using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère tout l'UI du jeu en VR :
///   • HUD gameplay compact (timer + barre + score) — coin haut-gauche
///   • Overlay centre (départ / game over + contrôles)
/// </summary>
public class HUDManager : MonoBehaviour
{
    // ── Refs runtime ──────────────────────────────────────────────────────────
    private TextMeshProUGUI timerLabel;
    private TextMeshProUGUI scoreLabel;
    private Image           timerFill;          // barre de progression
    private Image           timerFillTrack;

    private TextMeshProUGUI overTitle;
    private TextMeshProUGUI overScoreValue;
    private GameObject      overScoreRow;       // masqué sur l'écran de départ
    private TextMeshProUGUI overPrompt;

    private GameObject hudRoot;
    private GameObject overlayRoot;
    private Transform  hudCanvasT;
    private Transform  overlayCanvasT;

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BG       = new Color(0.03f, 0.05f, 0.09f, 0.93f);
    static readonly Color SHADOW   = new Color(0.00f, 0.00f, 0.00f, 0.42f);
    static readonly Color CYAN     = new Color(0.12f, 0.74f, 1.00f, 1.00f);
    static readonly Color CYAN_DIM = new Color(0.12f, 0.74f, 1.00f, 0.28f);
    static readonly Color AMBER    = new Color(1.00f, 0.72f, 0.08f, 1.00f);
    static readonly Color ORANGE   = new Color(1.00f, 0.42f, 0.04f, 1.00f);
    static readonly Color DANGER   = new Color(1.00f, 0.18f, 0.10f, 1.00f);
    static readonly Color WHITE    = new Color(0.92f, 0.96f, 1.00f, 1.00f);
    static readonly Color DIM      = new Color(0.38f, 0.53f, 0.66f, 1.00f);
    static readonly Color KEY_CLR  = new Color(1.00f, 0.80f, 0.12f, 1.00f);
    static readonly Color SEP      = new Color(0.11f, 0.20f, 0.30f, 0.85f);
    static readonly Color TRACK    = new Color(0.06f, 0.11f, 0.17f, 0.95f);

    // ── Build ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        var eye = GameObject.Find("CenterEyeAnchor");
        if (eye == null)
        {
            Debug.LogWarning("[HUDManager] CenterEyeAnchor introuvable.");
            eye = new GameObject("CenterEyeAnchor_Fallback");
        }

        BuildGameplayHUD(eye);
        BuildOverlay(eye);
        ShowStartScreen();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HUD Gameplay : timer + barre + score  (320 × 115, coin haut-gauche)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildGameplayHUD(GameObject eye)
    {
        var cv = CreateCanvas(eye, "HUD_Canvas",
            new Vector3(-0.60f, 0.42f, 2.5f), 0.0020f, new Vector2(320f, 115f));
        hudCanvasT = cv.transform;
        hudRoot    = cv;

        // — Couches de fond (du bas vers le haut) ——
        Img(cv, "Shadow",   new Vector2( 3f,  -3f),  new Vector2(320f, 115f), SHADOW);
        Img(cv, "BG",       Vector2.zero,             new Vector2(320f, 115f), BG);
        Img(cv, "LeftBar",  new Vector2(-158f,  0f),  new Vector2(3f,  109f),  CYAN);
        Img(cv, "TopBar",   new Vector2(  0f, 56f),   new Vector2(314f,  2f),  CYAN);
        Img(cv, "Divider",  new Vector2( 58f,  8f),   new Vector2(2f,   62f),  SEP);

        // — Barre de progression (piste + remplissage) —
        const float BX = -50f, BY = -28f, BW = 196f, BH = 6f;
        timerFillTrack = Img(cv, "BarTrack", new Vector2(BX, BY), new Vector2(BW, BH), TRACK);
        timerFill      = FillBar(cv, "BarFill", new Vector2(BX, BY), new Vector2(BW, BH), CYAN);

        // — Textes (rendus au-dessus des images) —
        TMP(cv, "TimeLabel", "TIME", 11,
            new Vector2(-68f, 40f), new Vector2(130f, 20f),
            TextAlignmentOptions.Left, DIM, false);

        timerLabel = TMP(cv, "TimerVal", "30.0", 44,
            new Vector2(-40f, 9f), new Vector2(175f, 52f),
            TextAlignmentOptions.Left, AMBER, true);

        TMP(cv, "ScoreLabel", "SCORE", 11,
            new Vector2(108f, 40f), new Vector2(100f, 20f),
            TextAlignmentOptions.Center, DIM, false);

        scoreLabel = TMP(cv, "ScoreVal", "0", 34,
            new Vector2(108f, 6f), new Vector2(100f, 46f),
            TextAlignmentOptions.Center, WHITE, true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Overlay Centre : départ / game over / contrôles  (440 × 360)
    // ─────────────────────────────────────────────────────────────────────────
    private void BuildOverlay(GameObject eye)
    {
        var cv = CreateCanvas(eye, "Overlay_Canvas",
            new Vector3(0f, 0.02f, 2.5f), 0.0038f, new Vector2(440f, 360f));
        overlayCanvasT = cv.transform;
        overlayRoot    = cv;

        // — Fond —
        Img(cv, "Shadow",  new Vector2( 4f,  -4f),  new Vector2(440f, 360f), SHADOW);
        Img(cv, "BG",      Vector2.zero,             new Vector2(440f, 360f), BG);
        Img(cv, "TopBar",  new Vector2(0f,  178f),   new Vector2(440f,  5f),  CYAN);
        Img(cv, "BotBar",  new Vector2(0f, -178f),   new Vector2(440f,  3f),  CYAN_DIM);

        // — Titre —
        overTitle = TMP(cv, "Title", "AIM TRAINER", 38,
            new Vector2(0f, 136f), new Vector2(420f, 56f),
            TextAlignmentOptions.Center, WHITE, true);

        // — Score (game over seulement) —
        overScoreRow = new GameObject("ScoreRow");
        overScoreRow.transform.SetParent(cv.transform, false);

        TMP(overScoreRow, "ScoreLabel", "SCORE FINAL", 12,
            new Vector2(0f, 78f), new Vector2(420f, 22f),
            TextAlignmentOptions.Center, DIM, false);

        overScoreValue = TMP(overScoreRow, "ScoreVal", "0", 54,
            new Vector2(0f, 38f), new Vector2(420f, 64f),
            TextAlignmentOptions.Center, AMBER, true);

        // — Prompt pulsant —
        overPrompt = TMP(cv, "Prompt",
            "APPUYER SUR  [ A ]  POUR COMMENCER", 18,
            new Vector2(0f, 4f), new Vector2(420f, 34f),
            TextAlignmentOptions.Center, WHITE, false);

        // — Séparateur + header contrôles —
        Img(cv, "Sep1", new Vector2(0f, -22f), new Vector2(400f, 2f), SEP);

        TMP(cv, "CtrlHeader", "— CONTRÔLES —", 11,
            new Vector2(0f, -38f), new Vector2(400f, 22f),
            TextAlignmentOptions.Center, DIM, false);

        // — Lignes de contrôles —
        float y = -62f;
        const float STEP = 24f;
        CtrlRow(cv, "R1", "[A]",          "Lancer / Rejouer",                 y);
        CtrlRow(cv, "R2", "[Y]",           "Ouvrir le menu armes",            y - STEP * 1);
        CtrlRow(cv, "R3", "[INDEX D]",      "Tirer",                           y - STEP * 2);
        CtrlRow(cv, "R4", "[GRIP D]",       "Ramasser / Poser l'arme",         y - STEP * 3);
        CtrlRow(cv, "R5", "[INDEX G]",      "Saisir la poignee avant",         y - STEP * 4);
    }

    private void CtrlRow(GameObject parent, string id, string key, string desc, float y)
    {
        // Colonne gauche (touches) : bord droit fixé à x = -30
        // Colonne droite (descriptions) : bord gauche fixé à x = -10  → gap 20px garanti
        TMP(parent, id + "_k", key,  13, new Vector2(-115f, y), new Vector2(170f, 21f),
            TextAlignmentOptions.Right,  KEY_CLR, true);
        TMP(parent, id + "_d", desc, 13, new Vector2( +90f, y), new Vector2(210f, 21f),
            TextAlignmentOptions.Left,   DIM,     false);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        var sm = ScoreManager.Instance;
        if (sm == null) return;
        sm.OnScoreChanged  += UpdateScore;
        sm.OnTimerTick     += UpdateTimer;
        sm.OnGameOver      += ShowGameOver;
        sm.OnGameRestarted += ShowHUD;
    }

    private void Update()
    {
        // Prompt pulsant sur l'overlay
        if (overlayRoot != null && overlayRoot.activeSelf && overPrompt != null)
            overPrompt.alpha = 0.50f + 0.50f * Mathf.Sin(Time.time * 2.4f);
    }

    private void LateUpdate()
    {
        // Canvas toujours face au regard, axe vertical stabilisé
        if (Camera.main == null) return;
        Quaternion upright = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        if (hudCanvasT     != null) hudCanvasT.rotation     = upright;
        if (overlayCanvasT != null) overlayCanvasT.rotation = upright;
    }

    // ── Transitions d'état ────────────────────────────────────────────────────

    private void ShowStartScreen()
    {
        hudRoot.SetActive(false);
        overlayRoot.SetActive(true);
        overTitle.text          = "AIM TRAINER";
        overPrompt.text         = "APPUYER SUR  [ A ]  POUR COMMENCER";
        overScoreRow.SetActive(false);
    }

    private void ShowHUD()
    {
        hudRoot.SetActive(true);
        overlayRoot.SetActive(false);
        UpdateScore();
        UpdateTimer();
    }

    private void ShowGameOver()
    {
        hudRoot.SetActive(false);
        overlayRoot.SetActive(true);
        overTitle.text  = "TEMPS ÉCOULÉ";
        overPrompt.text = "APPUYER SUR  [ A ]  POUR REJOUER";
        overScoreRow.SetActive(true);
        if (ScoreManager.Instance != null)
            overScoreValue.text = ScoreManager.Instance.Score.ToString();
    }

    private void UpdateScore()
    {
        if (scoreLabel != null && ScoreManager.Instance != null)
            scoreLabel.text = ScoreManager.Instance.Score.ToString();
    }

    private void UpdateTimer()
    {
        if (timerLabel == null || ScoreManager.Instance == null) return;

        float t     = ScoreManager.Instance.TimeRemaining;
        float total = ScoreManager.Instance.GameDuration;
        float ratio = Mathf.Clamp01(total > 0f ? t / total : 0f);

        timerLabel.text = t.ToString("F1");

        // Couleur : cyan → ambre → orange → rouge
        Color tc = ratio > 0.40f ? AMBER
                 : ratio > 0.20f ? ORANGE
                 : DANGER;

        timerLabel.color      = tc;
        if (timerFill != null) { timerFill.color = tc; timerFill.fillAmount = ratio; }
    }

    // ── Helpers UI ────────────────────────────────────────────────────────────

    private GameObject CreateCanvas(GameObject parent, string name,
                                    Vector3 localPos, float scale, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one * scale;
        var cv = go.AddComponent<Canvas>();
        cv.renderMode = RenderMode.WorldSpace;
        go.GetComponent<RectTransform>().sizeDelta = size;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private Image Img(GameObject parent, string name,
                      Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private Image FillBar(GameObject parent, string name,
                          Vector2 pos, Vector2 size, Color color)
    {
        var img = Img(parent, name, pos, size, color);
        img.type       = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        img.fillAmount = 1f;
        return img;
    }

    private TextMeshProUGUI TMP(GameObject parent, string name, string text,
                                float size, Vector2 pos, Vector2 sz,
                                TextAlignmentOptions align, Color color, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta        = sz;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text             = text;
        t.fontSize         = size;
        t.alignment        = align;
        t.color            = color;
        t.fontStyle        = bold ? FontStyles.Bold : FontStyles.Normal;
        t.enableWordWrapping = false;
        t.overflowMode     = TextOverflowModes.Overflow;
        return t;
    }
}
