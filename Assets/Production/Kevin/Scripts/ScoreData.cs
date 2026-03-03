using System.Collections.Generic;

namespace AimTrainer.Scoring
{
    /// <summary>
    /// Data container passed from GameManager to UI at game over.
    /// </summary>
    [System.Serializable]
    public class ScoreData
    {
        public int finalScore;
        public int highScore;
        public List<int> recentScores; // Top scores, sorted descending
    }
}
