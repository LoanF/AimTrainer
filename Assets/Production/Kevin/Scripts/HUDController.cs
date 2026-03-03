using TMPro;
using UnityEngine;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// World-space HUD shown during gameplay. Displays timer and current score.
    ///
    /// Setup in Unity:
    ///   1. Create a Canvas (Render Mode = World Space, scale 0.002)
    ///   2. Attach this script to the Canvas GameObject
    ///   3. Assign TimerText and ScoreText in the Inspector
    ///   4. Remove the GraphicRaycaster component (no buttons on HUD)
    ///   5. The canvas will lazy-follow the player's head automatically
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Follow Settings")]
        [SerializeField] private float followDistance = 2.0f;
        [SerializeField] private float verticalOffset = -0.2f;
        [SerializeField] private float followSpeed = 3f;

        [Header("Timer Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color urgentColor = Color.red;
        [SerializeField] private float urgentThreshold = 10f;

        private Transform _cameraTransform;

        // ── Programmatic setup ────────────────────────────────────────────────
        /// <summary>Called by UIBootstrapper to assign text references at runtime.</summary>
        public void Setup(TextMeshProUGUI timer, TextMeshProUGUI score)
        {
            timerText = timer;
            scoreText = score;
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            gameObject.SetActive(false); // Hidden until game starts
        }

        private void OnEnable()
        {
            GameManager.OnGameStart += HandleGameStart;
            GameManager.OnGameOver += HandleGameOver;
            GameManager.OnTimerUpdated += UpdateTimer;
            GameManager.OnScoreChanged += UpdateScore;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= HandleGameStart;
            GameManager.OnGameOver -= HandleGameOver;
            GameManager.OnTimerUpdated -= UpdateTimer;
            GameManager.OnScoreChanged -= UpdateScore;
        }

        private void Start()
        {
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            Vector3 targetPos = _cameraTransform.position
                + _cameraTransform.forward * followDistance
                + Vector3.up * verticalOffset;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.LookRotation(
                transform.position - _cameraTransform.position,
                _cameraTransform.up
            );
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void HandleGameStart()
        {
            gameObject.SetActive(true);
            UpdateScore(0);
        }

        private void HandleGameOver(ScoreData _)
        {
            gameObject.SetActive(false);
        }

        private void UpdateTimer(float secondsRemaining)
        {
            if (timerText == null) return;

            int seconds = Mathf.CeilToInt(secondsRemaining);
            timerText.text = seconds.ToString();
            timerText.color = seconds <= urgentThreshold ? urgentColor : normalColor;
        }

        private void UpdateScore(int score)
        {
            if (scoreText == null) return;
            scoreText.text = score.ToString();
        }
    }
}
