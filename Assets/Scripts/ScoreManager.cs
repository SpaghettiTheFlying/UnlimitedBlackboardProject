using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public int currentScore = 0;
    public int enemyKillPoints = 100;
    public int movementPoints = 1;
    public int collectiblePoints = 10;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;

    public TextMeshProUGUI levelText;

    public TextMeshProUGUI inGameHighScoreText;

    public RoundManager roundManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
        UpdateRoundUI();

        if (inGameHighScoreText != null)
        {
            inGameHighScoreText.text = $"Best: {GetHighScore()}";
        }
    }

    void Update()
    {
        UpdateRoundUI();
    }

    public void AddScore(int points, string reason = "")
    {
        currentScore += points;
        Debug.Log($"Puan kazanýldý: +{points} ({reason}). Toplam: {currentScore}");
        UpdateScoreUI();
    }

    public void OnEnemyKilled()
    {
        AddScore(enemyKillPoints, "Düþman yenildi");
    }

    public void OnPlayerMoved()
    {
        AddScore(movementPoints, "Hareket");
    }

    public void OnCollectibleGathered()
    {
        AddScore(collectiblePoints, "Obje toplandý");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    void UpdateRoundUI()
    {
        if (roundText != null && roundManager != null)
        {
            roundText.text = $"Round: {roundManager.GetCurrentRound()}/3";
        }

        if (levelText != null && roundManager != null)
        {
            levelText.text = $"Level: {roundManager.GetCurrentLevel()}";
        }
    }

    public int GetScore()
    {
        return currentScore;
    }

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }

    public bool CheckAndSaveHighScore()
    {
        int currentHighScore = GetHighScore();
        if (currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
            return true; // Evet, yeni rekor!
        }
        return false; // Hayýr, rekor kýrýlmadý.
    }
}