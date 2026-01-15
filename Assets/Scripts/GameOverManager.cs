using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullandýðýn için

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText; // "New High Score!" yazýsý

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Oyun baþýnda panel kapalý olsun
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        // Oyun içi zamaný durdur (arka planda hareket olmasýn)
        Time.timeScale = 0f;

        // Skor iþlemlerini yap
        int finalScore = ScoreManager.Instance.currentScore;
        bool isNewRecord = ScoreManager.Instance.CheckAndSaveHighScore();

        if (isNewRecord)
        {
            // Yeni rekor ise sadece tebrik mesajýný göster
            newHighScoreText.gameObject.SetActive(true);
            currentScoreText.gameObject.SetActive(false);
            highScoreText.gameObject.SetActive(false);

            newHighScoreText.text = $"NEW HIGH SCORE!\n{finalScore}";
        }
        else
        {
            // Rekor deðilse skorlarý alt alta göster
            newHighScoreText.gameObject.SetActive(false);
            currentScoreText.gameObject.SetActive(true);
            highScoreText.gameObject.SetActive(true);

            currentScoreText.text = $"Score: {finalScore}";
            highScoreText.text = $"High Score: {ScoreManager.Instance.GetHighScore()}";
        }
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; // Zamaný tekrar baþlat
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Bölümü yeniden yükle
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // 0 numaralý sahne (Ana Menü)
    }
}