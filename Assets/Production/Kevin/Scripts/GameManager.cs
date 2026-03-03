using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// Central singleton. Owns the game state machine, the 30s countdown,
    /// and all static events/methods used by the rest of the team.
    ///
    /// === API FOR OTHER TEAM MEMBERS ===
    ///
    /// Person 2 (Shooting) — call these from your shooting script:
    ///     GameManager.ReportHit();    // shot hit a target  → +1
    ///     GameManager.ReportMiss();   // shot hit nothing   → -1
    ///
    /// Person 3 (Respawn) — subscribe to know when a target was hit:
    ///     GameManager.OnTargetHit += YourRespawnMethod;   // in OnEnable
    ///     GameManager.OnTargetHit -= YourRespawnMethod;   // in OnDisable (required!)
    ///
    /// Anyone — subscribe to game lifecycle events:
    ///     GameManager.OnGameStart   += ...;
    ///     GameManager.OnGameOver    += ...;   // receives ScoreData
    ///     GameManager.OnTimerUpdated += ...;  // float seconds remaining
    ///     GameManager.OnScoreChanged += ...;  // int current score
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Events ────────────────────────────────────────────────────────────
        public static event Action OnGameStart;
        public static event Action<ScoreData> OnGameOver;
        public static event Action<float> OnTimerUpdated;
        public static event Action<int> OnScoreChanged;
        public static event Action OnTargetHit; // Optional hook for Person 3

        // ── State ─────────────────────────────────────────────────────────────
        public enum GameState { Idle, Playing, GameOver }
        public static GameState CurrentState { get; private set; } = GameState.Idle;

        public static GameManager Instance { get; private set; }

        // ── Config ────────────────────────────────────────────────────────────
        [SerializeField] private float gameDuration = 30f;

        // ── Internal ──────────────────────────────────────────────────────────
        private readonly ScoreManager _scoreManager = new ScoreManager();
        private Coroutine _countdownCoroutine;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _scoreManager.LoadScores();
        }

        private void Start()
        {
            StartGame();
        }

        private void OnApplicationPause(bool paused)
        {
            if (CurrentState != GameState.Playing) return;

            if (paused)
                StopCountdown();
            else
                ResumeCountdown();
        }

        // ── Game flow ─────────────────────────────────────────────────────────
        public void StartGame()
        {
            if (CurrentState == GameState.Playing) return;

            CurrentState = GameState.Playing;
            _scoreManager.ResetCurrentScore();
            OnGameStart?.Invoke();
            OnScoreChanged?.Invoke(_scoreManager.CurrentScore);

            StopCountdown();
            _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private void EndGame()
        {
            if (CurrentState != GameState.Playing) return;

            StopCountdown();
            CurrentState = GameState.GameOver;

            _scoreManager.SaveCurrentScore();
            ScoreData data = _scoreManager.BuildScoreData();
            OnGameOver?.Invoke(data);
        }

        private IEnumerator CountdownCoroutine()
        {
            float timeRemaining = gameDuration;

            while (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0f) timeRemaining = 0f;
                OnTimerUpdated?.Invoke(timeRemaining);
                yield return null;
            }

            EndGame();
        }

        private void StopCountdown()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
        }

        private void ResumeCountdown()
        {
            // Called after un-pause — restart from current timer value handled by OnTimerUpdated listeners
            _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        // ── Public API (called by other team members) ─────────────────────────

        /// <summary>Call this when a shot hits a target. +1 point.</summary>
        public static void ReportHit()
        {
            if (Instance == null || CurrentState != GameState.Playing) return;

            Instance._scoreManager.AddPoints(1);
            OnTargetHit?.Invoke();
            OnScoreChanged?.Invoke(Instance._scoreManager.CurrentScore);
        }

        /// <summary>Call this when a shot hits nothing. -1 point.</summary>
        public static void ReportMiss()
        {
            if (Instance == null || CurrentState != GameState.Playing) return;

            Instance._scoreManager.AddPoints(-1);
            OnScoreChanged?.Invoke(Instance._scoreManager.CurrentScore);
        }

        /// <summary>Reload the scene to restart the game cleanly.</summary>
        public static void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
