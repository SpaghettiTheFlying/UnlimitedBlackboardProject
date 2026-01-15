using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;   // "Continue, Options, Menu" butonlarýnýn olduðu panel
    public GameObject optionsPanel; // Ses ayarýnýn olduðu panel
    public Slider volumeSlider;     // Ses slider'ý

    void Start()
    {
        // Oyun baþýnda paneller kapalý olsun
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Slider'ý mevcut ses seviyesine eþitle (Eðer options açýksa)
        if (volumeSlider != null)
        {
            // Sesi logaritmikten normale çevirip slidera veriyoruz (görsel ayar)
            volumeSlider.value = Mathf.Sqrt(AudioListener.volume);
        }
    }

    // --- BUTON FONKSÝYONLARI ---

    // 1. Ekrandaki Durdurma Butonuna Baðla
    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Zamaný durdur
    }

    // 2. "Continue" Butonuna Baðla
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; // Zamaný devam ettir
    }

    // 3. "Main Menu" Butonuna Baðla
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Zamaný düzeltmeyi unutma!
        SceneManager.LoadScene(0); // 0 = Ana Menü
    }

    // 4. "Options" Butonuna Baðla
    public void OpenOptions()
    {
        pausePanel.SetActive(false); // Pause menüsünü gizle
        optionsPanel.SetActive(true); // Ayarlarý aç
    }

    // 5. Options içindeki "Back" Butonuna Baðla
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true); // Geri Pause menüsüne dön
    }

    // 6. Slider'a Baðla (Dynamic float)
    public void SetVolume(float volume)
    {
        // Logaritmik ses ayarý (Daha doðal kýsma hissi)
        AudioListener.volume = Mathf.Pow(volume, 2);
    }
}