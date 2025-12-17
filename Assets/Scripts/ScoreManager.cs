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
    public TextMeshProUGUI scoreText; // veya public Text scoreText;

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

    public int GetScore()
    {
        return currentScore;
    }
}