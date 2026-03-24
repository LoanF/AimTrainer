using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu de sélection d'arme en VR.
///   [Y] gauche          : ouvrir / fermer (hors partie uniquement)
///   Main droite + laser : pointer une carte
///   [Index D]           : confirmer la sélection
///
/// Design : cartes flottantes qui suivent la tête, chacune avec une couleur d'accent unique.
/// </summary>
public class WeaponSelectionMenu : MonoBehaviour
{
    [Header("Armes disponibles")]
    [Tooltip("Glisser ici les prefabs WeaponPickup (pas besoin de les mettre dans la scène)")]
    [SerializeField] private WeaponPickup[] weaponPrefabs;

    // Instances réelles créées au démarrage (invisibles jusqu'à sélection)
    private WeaponPickup[] weapons;

    // ── État ─────────────────────────────────────────────────────────────────
    public bool IsOpen { get; private set; }
    private int   hoveredIndex = -1;
    private float triggerPrev;

    // ── Références ───────────────────────────────────────────────────────────
    private Transform    rightHandT;
    private WeaponHolder weaponHolder;

    // ── Objets UI ─────────────────────────────────────────────────────────────
    private GameObject[]       cardRoots;
    private BoxCollider[]       cardColliders;
    private Image[]             cardBG;
    private Image[]             cardBorder;
    private TextMeshProUGUI[]   cardNames;
    private LineRenderer        laser;

    // ── Disposition (world-space, recalculée depuis l'œil chaque frame) ──────
    private const float CARD_DEPTH  = 2.0f;
    private const float CARD_SPREAD = 0.52f;
    private const float CARD_W      = 0.42f;
    private const float CARD_H      = 0.56f;
    private const float CARD_Y_OFF  = -0.12f;

    private const float CANVAS_SCALE = 0.001f;
    private const int   CANVAS_W     = 420;
    private const int   CANVAS_H     = 560;

    // ── Palette globale ───────────────────────────────────────────────────────
    static readonly Color BG_IDLE    = new Color(0.04f, 0.06f, 0.11f, 0.94f);
    static readonly Color BG_HOVER   = new Color(0.08f, 0.11f, 0.18f, 0.97f);
    static readonly Color BDR_IDLE   = new Color(0.15f, 0.24f, 0.34f, 0.85f);
    static readonly Color NAME_IDLE  = new Color(0.90f, 0.94f, 1.00f, 1.00f);
    static readonly Color NAME_HOVER = new Color(1.00f, 1.00f, 1.00f, 1.00f);
    static readonly Color STATS_CLR  = new Color(0.55f, 0.68f, 0.78f, 1.00f);
    static readonly Color DIM        = new Color(0.32f, 0.44f, 0.55f, 1.00f);
    static readonly Color SEP_CLR    = new Color(0.14f, 0.24f, 0.32f, 0.85f);

    // Couleur d'accent unique par arme (cyan, orange, vert, violet, or)
    static readonly Color[] ACCENTS = new Color[]
    {
        new Color(0.12f, 0.74f, 1.00f, 1.00f),
        new Color(1.00f, 0.48f, 0.05f, 1.00f),
        new Color(0.32f, 1.00f, 0.42f, 1.00f),
        new Color(0.72f, 0.18f, 1.00f, 1.00f),
        new Color(1.00f, 0.82f, 0.10f, 1.00f),
    };

    // ════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        var rha = GameObject.Find("RightHandAnchor");
        if (rha != null) rightHandT = rha.transform;

        weaponHolder = FindFirstObjectByType<WeaponHolder>();

        // Les instances doivent exister avant la construction des cartes
        InstantiateWeapons();
        BuildAllCards();
        SetCardsActive(false);
        BuildLaser();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnGameRestarted += ForceClose;
    }

    private void InstantiateWeapons()
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogError("[WeaponMenu] Aucun prefab dans 'Weapon Prefabs' — assigne-les dans l'Inspector !");
            weapons = new WeaponPickup[0];
            return;
        }

        weapons = new WeaponPickup[weaponPrefabs.Length];
        for (int i = 0; i < weaponPrefabs.Length; i++)
        {
            if (weaponPrefabs[i] == null) continue;
            // Instancié hors scène, désactivé — sera activé à la sélection
            weapons[i] = Instantiate(weaponPrefabs[i], Vector3.zero, Quaternion.identity);
            weapons[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // [Y] gauche → toggle, hors partie seulement
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            bool playing = ScoreManager.Instance != null && ScoreManager.Instance.IsPlaying;
            if (!playing) { if (IsOpen) CloseMenu(); else OpenMenu(); }
        }

        if (!IsOpen) return;

        // Le menu suit la tête : recalcul chaque frame
        PositionCards();

        // Animation de scale fluide sur le hover
        for (int i = 0; i < cardRoots.Length; i++)
        {
            if (cardRoots[i] == null) continue;
            float target = (i == hoveredIndex) ? 1.07f : 1.00f;
            float cur    = cardRoots[i].transform.localScale.x;
            cardRoots[i].transform.localScale =
                Vector3.one * Mathf.Lerp(cur, target, Time.deltaTime * 14f);
        }

        UpdateRaycast();
        UpdateSelection();
    }

    // ── Raycast ───────────────────────────────────────────────────────────────

    private void UpdateRaycast()
    {
        if (rightHandT == null)
        {
            var rha = GameObject.Find("RightHandAnchor");
            if (rha != null) rightHandT = rha.transform; else return;
        }

        Vector3 handPos = rightHandT.position;
        Vector3 handFwd = rightHandT.forward;

        // Détection angulaire : carte la plus proche de l'axe de pointage
        int   newHover  = -1;
        float bestAngle = 20f; // seuil en degrés

        for (int i = 0; i < cardRoots.Length; i++)
        {
            if (cardRoots[i] == null || !cardRoots[i].activeSelf) continue;
            float angle = Vector3.Angle(handFwd, cardRoots[i].transform.position - handPos);
            if (angle < bestAngle) { bestAngle = angle; newHover = i; }
        }

        if (newHover != hoveredIndex)
        {
            if (hoveredIndex >= 0) SetHover(hoveredIndex, false);
            hoveredIndex = newHover;
            if (hoveredIndex >= 0) SetHover(hoveredIndex, true);
        }

        laser.SetPosition(0, handPos);
        laser.SetPosition(1, hoveredIndex >= 0
            ? cardRoots[hoveredIndex].transform.position
            : handPos + handFwd * 5f);
    }

    private void UpdateSelection()
    {
        // Seuil analogique (0.55) plus fiable que le bouton binaire (full press)
        float t = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        bool  pressed = t > 0.55f && triggerPrev <= 0.55f;
        triggerPrev = t;

        if (pressed && hoveredIndex >= 0)
            ConfirmSelection(hoveredIndex);
    }

    private void ConfirmSelection(int index)
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("[WeaponMenu] Le tableau 'Weapons' est vide — assigne les WeaponPickup dans l'Inspector !");
            return;
        }
        if (index >= weapons.Length || weapons[index] == null)
        {
            Debug.LogError($"[WeaponMenu] weapons[{index}] est null !");
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i] != null)
                weapons[i].gameObject.SetActive(i == index);

        if (weaponHolder == null) weaponHolder = FindFirstObjectByType<WeaponHolder>();
        if (weaponHolder == null)
        {
            Debug.LogError("[WeaponMenu] WeaponHolder introuvable dans la scène !");
            CloseMenu();
            return;
        }

        weaponHolder.ForceEquip(weapons[index]);
        CloseMenu();
    }

    // ── Ouverture / fermeture ─────────────────────────────────────────────────

    private void OpenMenu()
    {
        IsOpen       = true;
        hoveredIndex = -1;
        // Sync triggerPrev avec l'état actuel — évite le faux déclenchement
        // si le trigger était déjà pressé (tir précédent, etc.)
        triggerPrev  = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        SetCardsActive(true);
    }

    private void CloseMenu()
    {
        IsOpen = false;
        if (hoveredIndex >= 0) { SetHover(hoveredIndex, false); hoveredIndex = -1; }
        SetCardsActive(false);
    }

    private void ForceClose() => CloseMenu();

    // ── Positionnement (suit la tête chaque frame) ────────────────────────────

    private void PositionCards()
    {
        var eyeGO = GameObject.Find("CenterEyeAnchor");
        if (eyeGO == null || cardRoots == null) return;

        Transform eye = eyeGO.transform;
        Vector3   fwd = eye.forward; fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
        fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        int   count  = cardRoots.Length;
        float totalW = (count - 1) * CARD_SPREAD;

        for (int i = 0; i < count; i++)
        {
            if (cardRoots[i] == null) continue;
            float xOff = -totalW * 0.5f + i * CARD_SPREAD;
            cardRoots[i].transform.position = eye.position
                + fwd   * CARD_DEPTH
                + right * xOff
                + Vector3.up * CARD_Y_OFF;
            // LookRotation(+fwd) : le Canvas se lit correctement depuis le côté -Z local (côté joueur)
            cardRoots[i].transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

    // ── Hover ─────────────────────────────────────────────────────────────────

    private void SetHover(int i, bool on)
    {
        if (i < 0 || i >= cardRoots.Length) return;
        Color accent = WeaponAccent(i);
        if (cardBG[i]     != null) cardBG[i].color     = on ? BG_HOVER  : BG_IDLE;
        if (cardBorder[i] != null) cardBorder[i].color  = on ? accent    : BDR_IDLE;
        if (cardNames[i]  != null) cardNames[i].color   = on ? accent    : NAME_IDLE;
    }

    private void SetCardsActive(bool on)
    {
        if (cardRoots != null)
            foreach (var c in cardRoots)
                if (c != null) c.SetActive(on);
        if (laser != null) laser.gameObject.SetActive(on);
    }

    // ── Construction des cartes ───────────────────────────────────────────────

    private void BuildAllCards()
    {
        int n       = weapons != null ? weapons.Length : 0;
        cardRoots   = new GameObject[n];
        cardColliders = new BoxCollider[n];
        cardBG      = new Image[n];
        cardBorder  = new Image[n];
        cardNames   = new TextMeshProUGUI[n];
        for (int i = 0; i < n; i++) BuildCard(i);
    }

    private void BuildCard(int i)
    {
        var  data  = weapons[i] != null ? weapons[i].weaponData : null;
        var  name  = data != null ? data.weaponName  : $"ARME {i + 1}";
        var  mode  = data != null ? data.fireMode.ToString().ToUpper() : "—";
        float rps  = data != null && data.shootCooldown > 0f ? 1f / data.shootCooldown : 0f;
        var  hands = data != null ? (data.isTwoHanded ? "DEUX MAINS" : "UNE MAIN") : "";
        string dots = PowerDots(rps);
        Color accent = WeaponAccent(i);

        // ── Root + Collider ──────────────────────────────────────────────────
        var root = new GameObject($"WeaponCard_{i}");
        root.transform.SetParent(transform, false);
        cardRoots[i] = root;

        var col = root.AddComponent<BoxCollider>();
        col.size      = new Vector3(CARD_W, CARD_H, 0.05f);
        col.isTrigger = true;
        cardColliders[i] = col;

        // ── Canvas WorldSpace ────────────────────────────────────────────────
        var cvGO = new GameObject("Canvas");
        cvGO.transform.SetParent(root.transform, false);
        cvGO.transform.localPosition = Vector3.zero;
        cvGO.transform.localRotation = Quaternion.identity;
        cvGO.transform.localScale    = Vector3.one * CANVAS_SCALE;

        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        cvGO.GetComponent<RectTransform>().sizeDelta = new Vector2(CANVAS_W, CANVAS_H);

        // ── Couches de fond ──────────────────────────────────────────────────
        // Ombre
        cardBorder[i] = MakeImgRect(cvGO, "Shadow", 0,
            new Vector2(4f, -4f), Vector2.zero, new Color(0f, 0f, 0f, 0.50f));

        // Fond principal
        cardBG[i] = MakeImgRect(cvGO, "BG", 1,
            Vector2.zero, Vector2.zero, BG_IDLE);

        // Bordure extérieure (légère)
        var bdr = MakeImgRect(cvGO, "Border", 2,
            new Vector2(-8f, -8f), new Vector2(8f, 8f), BDR_IDLE);
        cardBorder[i] = bdr;

        // Barre d'accent haut (couleur unique par arme)
        MakeRawRect(cvGO, "AccentTop", 3,
            new Vector2(0f, 276f), new Vector2(CANVAS_W, 8f), accent);

        // Subtile barre basse
        MakeRawRect(cvGO, "AccentBot", 4,
            new Vector2(0f, -276f), new Vector2(CANVAS_W, 3f),
            new Color(accent.r, accent.g, accent.b, 0.35f));

        // ── Contenu ─────────────────────────────────────────────────────────

        // Initiale de l'arme (grande lettre décorative en fond)
        string initial = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "W";
        CardTMP(cvGO, "Initial", initial, 110,
            new Vector2(0f, 180f), new Vector2(CANVAS_W, 160f),
            TextAlignmentOptions.Center,
            new Color(accent.r, accent.g, accent.b, 0.12f), true);

        // Nom de l'arme
        cardNames[i] = CardTMP(cvGO, "Name", name.ToUpper(), 48,
            new Vector2(0f, 115f), new Vector2(CANVAS_W - 40f, 120f),
            TextAlignmentOptions.Center, NAME_IDLE, true);

        // Séparateur
        MakeRawRect(cvGO, "Sep1", 10,
            new Vector2(0f, 40f), new Vector2(CANVAS_W - 80f, 2f), SEP_CLR);

        // Mode de tir
        CardTMP(cvGO, "Mode", mode, 28,
            new Vector2(0f, 0f), new Vector2(CANVAS_W - 40f, 44f),
            TextAlignmentOptions.Center, STATS_CLR, false);

        // Cadence
        string rpsStr = rps > 0f ? $"{rps:F1}  RPS" : "—";
        CardTMP(cvGO, "Rate", rpsStr, 24,
            new Vector2(0f, -52f), new Vector2(CANVAS_W - 40f, 38f),
            TextAlignmentOptions.Center,
            new Color(accent.r, accent.g, accent.b, 0.90f), true);

        // Indicateur de cadence (pastilles)
        CardTMP(cvGO, "Dots", dots, 20,
            new Vector2(0f, -105f), new Vector2(CANVAS_W - 40f, 32f),
            TextAlignmentOptions.Center, DIM, false);

        // Séparateur
        MakeRawRect(cvGO, "Sep2", 11,
            new Vector2(0f, -138f), new Vector2(CANVAS_W - 80f, 2f), SEP_CLR);

        // Prise en main
        CardTMP(cvGO, "Hands", hands, 20,
            new Vector2(0f, -170f), new Vector2(CANVAS_W - 40f, 34f),
            TextAlignmentOptions.Center, DIM, false);

        // Indice de sélection
        CardTMP(cvGO, "Hint", "POINTER  +  [ INDEX D ]", 15,
            new Vector2(0f, -230f), new Vector2(CANVAS_W - 40f, 30f),
            TextAlignmentOptions.Center,
            new Color(0.30f, 0.40f, 0.50f, 0.85f), false);
    }

    // ── Laser ─────────────────────────────────────────────────────────────────

    private void BuildLaser()
    {
        var go = new GameObject("MenuLaser");
        go.transform.SetParent(transform, false);
        laser = go.AddComponent<LineRenderer>();
        laser.positionCount     = 2;
        laser.useWorldSpace     = true;
        laser.startWidth        = 0.005f;
        laser.endWidth          = 0.001f;
        laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows    = false;

        Shader sh = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        if (sh != null) laser.material = new Material(sh);

        laser.startColor = new Color(0.70f, 0.90f, 1.00f, 0.90f);
        laser.endColor   = new Color(1.00f, 1.00f, 0.35f, 0.25f);
        go.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Color WeaponAccent(int i) =>
        ACCENTS[i % ACCENTS.Length];

    private static string PowerDots(float rps)
    {
        int n = Mathf.Clamp(Mathf.RoundToInt(rps), 0, 10);
        return new string('●', n) + new string('○', 10 - n);
    }

    // Image étirée aux bords du canvas (anchorMin=0, anchorMax=1)
    private Image MakeImgRect(GameObject parent, string name, int sibIdx,
                              Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        go.transform.SetSiblingIndex(sibIdx);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    // Rectangle à position/taille fixes (pivot centre)
    private Image MakeRawRect(GameObject parent, string name, int sibIdx,
                              Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.SetSiblingIndex(sibIdx);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private TextMeshProUGUI CardTMP(GameObject parent, string name, string text,
                                    float size, Vector2 pos, Vector2 sz,
                                    TextAlignmentOptions align, Color color, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = sz;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text               = text;
        t.fontSize           = size;
        t.alignment          = align;
        t.color              = color;
        t.fontStyle          = bold ? FontStyles.Bold : FontStyles.Normal;
        t.enableWordWrapping = false;
        t.overflowMode       = TextOverflowModes.Overflow;
        return t;
    }
}
