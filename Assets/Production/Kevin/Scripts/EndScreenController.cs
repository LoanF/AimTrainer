using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// World-space end screen shown after the 30s timer expires.
    ///
    /// Setup in Unity:
    ///   1. Create a Canvas (Render Mode = World Space, scale 0.002)
    ///      Position it ~2m in front of the player spawn point, at eye level
    ///   2. Attach this script to the Canvas GameObject
    ///   3. Assign all text fields and the Play Again button in the Inspector
    ///   4. Wire PlayAgainButton.onClick → OnPlayAgainPressed()
    ///   5. The canvas starts inactive and becomes visible at game over
    /// </summary>
    public class EndScreenController : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("Recent Scores (up to 5 entries)")]
        [SerializeField] private List<TextMeshProUGUI> recentScoreTexts;

        [Header("Button")]
        [SerializeField] private Button playAgainButton;

        // ── Programmatic setup ────────────────────────────────────────────────
        /// <summary>Called by UIBootstrapper to assign all references at runtime.</summary>
        public void Setup(TextMeshProUGUI finalScore, TextMeshProUGUI highScore,
                          List<TextMeshProUGUI> recentScores, Button playAgain)
        {
            finalScoreText = finalScore;
            highScoreText = highScore;
            recentScoreTexts = recentScores;
            playAgainButton = playAgain;
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GameManager.OnGameOver += ShowResults;
        }

        private void OnDisable()
        {
            GameManager.OnGameOver -= ShowResults;
        }

        private void Start()
        {
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainPressed);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void ShowResults(ScoreData data)
        {
            gameObject.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = data.finalScore.ToString();

            if (highScoreText != null)
                highScoreText.text = data.highScore.ToString();

            for (int i = 0; i < recentScoreTexts.Count; i++)
            {
                if (recentScoreTexts[i] == null) continue;

                if (i < data.recentScores.Count)
                    recentScoreTexts[i].text = (i + 1) + ". " + data.recentScores[i];
                else
                    recentScoreTexts[i].text = "-";
            }
        }

        // ── Button callback ───────────────────────────────────────────────────
        public void OnPlayAgainPressed()
        {
            GameManager.RestartGame();
        }
    }
}
