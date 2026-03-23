using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] public float GameDuration = 30f;

    public int Score { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsPlaying { get; private set; }

    public Action OnScoreChanged;
    public Action OnTimerTick;
    public Action OnGameOver;
    public Action OnGameRestarted;

    private TargetSpawner spawner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        spawner = FindFirstObjectByType<TargetSpawner>();

        if (spawner != null)
            spawner.CurrentTarget.OnHit += RegisterHit;

        OnGameOver += () =>
        {
            if (spawner != null)
            {
                spawner.IsEnabled = false;
                spawner.SetTargetVisible(false);
            }
        };

        OnGameRestarted += () =>
        {
            if (spawner != null)
            {
                spawner.IsEnabled = true;
                spawner.SetTargetVisible(true);
                spawner.Respawn();
            }
        };
    }

    private void Update()
    {
        // Bouton A (droit) pour démarrer / redémarrer
        if (!IsPlaying && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            StartGame();

        if (!IsPlaying) return;

        TimeRemaining -= Time.deltaTime;
        OnTimerTick?.Invoke();

        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsPlaying = false;
            OnTimerTick?.Invoke();
            OnGameOver?.Invoke();
        }
    }

    public void StartGame()
    {
        Score = 0;
        TimeRemaining = GameDuration;
        IsPlaying = true;
        OnGameRestarted?.Invoke();
        OnScoreChanged?.Invoke();
        OnTimerTick?.Invoke();
    }

    public void RegisterHit()
    {
        if (!IsPlaying) return;
        Score++;
        OnScoreChanged?.Invoke();
    }
}
