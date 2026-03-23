using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private Text scoreText;
    private Text timerText;
    private Text gameOverScoreText;
    private GameObject hudPanel;
    private GameObject gameOverPanel;

    private void Awake()
    {
        GameObject centerEye = GameObject.Find("CenterEyeAnchor");
        if (centerEye == null)
        {
            Debug.LogWarning("[HUDManager] CenterEyeAnchor not found. HUD will not be positioned correctly.");
            centerEye = new GameObject("CenterEyeAnchor_Fallback");
        }

        // Small top-left canvas for score + timer during gameplay
        GameObject hudCanvasGO = CreateCanvas(centerEye, "HUDCanvas",
            position: new Vector3(-0.72f, 0.52f, 2.5f),
            scale: 0.0022f,
            size: new Vector2(260f, 90f));

        hudPanel = CreatePanel(hudCanvasGO, "HUDPanel", Vector2.zero, new Vector2(260f, 90f));
        timerText = CreateText(hudPanel, "TimerText", "30.0", 52, new Vector2(0f, 12f), new Vector2(240f, 60f), TextAnchor.MiddleLeft);
        scoreText = CreateText(hudPanel, "ScoreText", "Score: 0", 22, new Vector2(4f, -28f), new Vector2(240f, 34f), TextAnchor.MiddleLeft);

        // Centered canvas for start screen and game over
        GameObject overlayCanvasGO = CreateCanvas(centerEye, "OverlayCanvas",
            position: new Vector3(0f, 0f, 2.5f),
            scale: 0.005f,
            size: new Vector2(400f, 200f));

        gameOverPanel = CreatePanel(overlayCanvasGO, "GameOverPanel", Vector2.zero, new Vector2(400f, 200f));
        CreateText(gameOverPanel, "GameOverTitle", "AIM TRAINER", 36, new Vector2(0f, 60f), new Vector2(380f, 60f), TextAnchor.MiddleCenter);
        gameOverScoreText = CreateText(gameOverPanel, "GameOverScore", "", 28, new Vector2(0f, 10f), new Vector2(380f, 50f), TextAnchor.MiddleCenter);
        CreateText(gameOverPanel, "GameOverPrompt", "Press A to start", 20, new Vector2(0f, -50f), new Vector2(380f, 40f), TextAnchor.MiddleCenter);

        ShowStartScreen();
    }

    private void Start()
    {
        ScoreManager sm = ScoreManager.Instance;
        if (sm != null)
        {
            sm.OnScoreChanged += UpdateScore;
            sm.OnTimerTick += UpdateTimer;
            sm.OnGameOver += ShowGameOver;
            sm.OnGameRestarted += ShowHUD;
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            if (ScoreManager.Instance != null && !ScoreManager.Instance.IsPlaying)
                ScoreManager.Instance.StartGame();
        }
    }

    private void ShowStartScreen()
    {
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        Text t1 = gameOverPanel.transform.Find("GameOverTitle")?.GetComponent<Text>();
        if (t1 != null) t1.text = "AIM TRAINER";
        Text p1 = gameOverPanel.transform.Find("GameOverPrompt")?.GetComponent<Text>();
        if (p1 != null) p1.text = "Press A to start";
        if (gameOverScoreText != null) gameOverScoreText.text = "";
    }

    private void ShowHUD()
    {
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        UpdateScore();
        UpdateTimer();
    }

    private void ShowGameOver()
    {
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        Text t2 = gameOverPanel.transform.Find("GameOverTitle")?.GetComponent<Text>();
        if (t2 != null) t2.text = "TIME'S UP";
        Text p2 = gameOverPanel.transform.Find("GameOverPrompt")?.GetComponent<Text>();
        if (p2 != null) p2.text = "Press A to restart";

        if (gameOverScoreText != null && ScoreManager.Instance != null)
            gameOverScoreText.text = $"Score: {ScoreManager.Instance.Score}";
    }

    private void UpdateScore()
    {
        if (scoreText != null && ScoreManager.Instance != null)
            scoreText.text = $"Score: {ScoreManager.Instance.Score}";
    }

    private void UpdateTimer()
    {
        if (timerText != null && ScoreManager.Instance != null)
            timerText.text = ScoreManager.Instance.TimeRemaining.ToString("F1");
    }

    private GameObject CreateCanvas(GameObject parent, string name, Vector3 position, float scale, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * scale;

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        go.GetComponent<RectTransform>().sizeDelta = size;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private GameObject CreatePanel(GameObject parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        return panel;
    }

    private Text CreateText(GameObject parent, string name, string content, int fontSize, Vector2 anchoredPos, Vector2 size, TextAnchor alignment)
    {
        GameObject textGO = new GameObject(name);
        textGO.transform.SetParent(parent.transform, false);
        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Text text = textGO.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }
}
