using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// Generates the entire UI (HUD + EndScreen) at runtime programmatically.
    ///
    /// === SETUP (une seule étape) ===
    ///   1. Window > TextMeshPro > Import TMP Essential Resources  (si pas déjà fait)
    ///   2. Créer un GameObject vide dans la scène → Add Component > UIBootstrapper
    ///   3. Lancer le jeu → tout se crée automatiquement
    /// </summary>
    public class UIBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            CreateHUD();
            CreateEndScreen();
            EnsureGameManager();
        }

        // ── HUD ──────────────────────────────────────────────────────────────
        private void CreateHUD()
        {
            // Canvas
            GameObject canvasGO = new GameObject("HUD_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            // No GraphicRaycaster — HUD has no interactive elements

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(800, 200);
            canvasGO.transform.localScale = Vector3.one * 0.002f;

            TMP_FontAsset font = LoadTMPFont();

            // Timer text (left side)
            TextMeshProUGUI timerTMP = CreateTMPText(canvasGO, "TimerText", font,
                new Vector2(-150, 0), new Vector2(300, 150), 80);
            timerTMP.text = "30";
            timerTMP.alignment = TextAlignmentOptions.Center;

            // Score text (right side)
            TextMeshProUGUI scoreTMP = CreateTMPText(canvasGO, "ScoreText", font,
                new Vector2(150, 0), new Vector2(300, 150), 80);
            scoreTMP.text = "0";
            scoreTMP.alignment = TextAlignmentOptions.Center;

            // Attach controller and wire references
            HUDController hudController = canvasGO.AddComponent<HUDController>();
            hudController.Setup(timerTMP, scoreTMP);
        }

        // ── End Screen ───────────────────────────────────────────────────────
        private void CreateEndScreen()
        {
            // Canvas
            GameObject canvasGO = new GameObject("EndScreen_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>(); // Needed for button interaction

            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(800, 600);
            canvasGO.transform.localScale = Vector3.one * 0.002f;
            canvasGO.transform.position = new Vector3(0f, 1.6f, 2.5f); // Fixed 2.5m in front

            TMP_FontAsset font = LoadTMPFont();

            // Background panel
            GameObject bg = CreatePanel(canvasGO, "Background", new Color(0f, 0f, 0f, 0.85f),
                Vector2.zero, new Vector2(800, 600));

            // Title
            CreateTMPText(bg, "TitleText", font, new Vector2(0, 220), new Vector2(700, 80), 60)
                .text = "GAME OVER";

            // Final score
            CreateTMPText(bg, "FinalScoreLabel", font, new Vector2(0, 130), new Vector2(600, 50), 28)
                .text = "VOTRE SCORE";
            TextMeshProUGUI finalScoreTMP = CreateTMPText(bg, "FinalScoreValue", font,
                new Vector2(0, 75), new Vector2(600, 70), 56);
            finalScoreTMP.text = "0";

            // High score
            CreateTMPText(bg, "HighScoreLabel", font, new Vector2(0, 15), new Vector2(600, 40), 24)
                .text = "MEILLEUR SCORE";
            TextMeshProUGUI highScoreTMP = CreateTMPText(bg, "HighScoreValue", font,
                new Vector2(0, -30), new Vector2(600, 50), 36);
            highScoreTMP.text = "0";

            // Recent scores (5 slots)
            CreateTMPText(bg, "RecentLabel", font, new Vector2(0, -80), new Vector2(600, 36), 20)
                .text = "SCORES RÉCENTS";

            var recentTexts = new List<TextMeshProUGUI>();
            for (int i = 0; i < 5; i++)
            {
                float y = -115 - i * 38;
                TextMeshProUGUI entry = CreateTMPText(bg, "Recent_" + (i + 1), font,
                    new Vector2(0, y), new Vector2(400, 34), 22);
                entry.text = "-";
                recentTexts.Add(entry);
            }

            // Play Again button
            Button playAgainBtn = CreateButton(bg, "PlayAgainButton", font,
                new Vector2(0, -310), new Vector2(280, 60), "REJOUER");
            playAgainBtn.onClick.AddListener(GameManager.RestartGame);

            // Attach controller and wire references
            EndScreenController endCtrl = canvasGO.AddComponent<EndScreenController>();
            endCtrl.Setup(finalScoreTMP, highScoreTMP, recentTexts, playAgainBtn);
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;

            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static TMP_FontAsset LoadTMPFont()
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font == null)
                Debug.LogWarning("[UIBootstrapper] TMP font not found. Run: " +
                    "Window > TextMeshPro > Import TMP Essential Resources");
            return font;
        }

        private static TextMeshProUGUI CreateTMPText(GameObject parent, string name,
            TMP_FontAsset font, Vector2 anchoredPos, Vector2 sizeDelta, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            return tmp;
        }

        private static GameObject CreatePanel(GameObject parent, string name, Color color,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            Image img = go.AddComponent<Image>();
            img.color = color;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            return go;
        }

        private static Button CreateButton(GameObject parent, string name, TMP_FontAsset font,
            Vector2 anchoredPos, Vector2 sizeDelta, string label)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 1f);

            Button btn = go.AddComponent<Button>();

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            // Button label
            TextMeshProUGUI tmp = CreateTMPText(go, "Label", font, Vector2.zero, sizeDelta, 26);
            tmp.text = label;
            tmp.color = Color.white;

            return btn;
        }
    }
}
