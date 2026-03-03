using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// Handles score arithmetic and persistence via PlayerPrefs.
    /// NOTE: Always call PlayerPrefs.Save() after writing — required on Android/Quest.
    /// </summary>
    public class ScoreManager
    {
        private const string ScoresKey = "AimTrainer_Scores";
        private const int MaxSavedScores = 10;

        private List<int> _savedScores = new List<int>();

        public int CurrentScore { get; private set; }
        public IReadOnlyList<int> SavedScores => _savedScores.AsReadOnly();
        public int HighScore => _savedScores.Count > 0 ? _savedScores[0] : 0; // List is sorted descending

        public void ResetCurrentScore()
        {
            CurrentScore = 0;
        }

        public void AddPoints(int delta)
        {
            CurrentScore += delta;
        }

        public void LoadScores()
        {
            _savedScores.Clear();
            string raw = PlayerPrefs.GetString(ScoresKey, "");
            if (string.IsNullOrEmpty(raw)) return;

            foreach (string part in raw.Split(','))
            {
                if (int.TryParse(part.Trim(), out int score))
                    _savedScores.Add(score);
            }

            _savedScores.Sort((a, b) => b.CompareTo(a)); // Descending
        }

        public void SaveCurrentScore()
        {
            _savedScores.Add(CurrentScore);
            _savedScores.Sort((a, b) => b.CompareTo(a));

            if (_savedScores.Count > MaxSavedScores)
                _savedScores = _savedScores.GetRange(0, MaxSavedScores);

            PlayerPrefs.SetString(ScoresKey, string.Join(",", _savedScores));
            PlayerPrefs.Save(); // Critical on Android: forces flush to disk
        }

        public ScoreData BuildScoreData()
        {
            return new ScoreData
            {
                finalScore = CurrentScore,
                highScore = HighScore,
                recentScores = new List<int>(_savedScores.Take(5))
            };
        }
    }
}
