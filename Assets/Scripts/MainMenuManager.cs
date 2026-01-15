using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject howToPlayPanel;

    [Header("Settings")]
    public Slider volumeSlider;

    private void Start()
    {
        
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        StartCoroutine(PlayMusicWithDelay());
    }

    IEnumerator PlayMusicWithDelay()
    {       
        if (SoundManager.Instance != null && SoundManager.Instance.menuMusic != null)
        {           
            SoundManager.Instance.musicSource.loop = false;

            while (true) 
            {          
                SoundManager.Instance.PlayMusic(SoundManager.Instance.menuMusic);

                float musicLength = SoundManager.Instance.menuMusic.length;
                yield return new WaitForSeconds(musicLength);
                
                yield return new WaitForSeconds(3f);   
            }
        }
    }

    // --- BUTON FONKSÝYONLARI ---

    public void PlayGame()
    {
        
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýldý!");
        Application.Quit();
    }

    // --- PANEL GEÇÝÞLERÝ ---

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenHowToPlay()
    {
        optionsPanel.SetActive(false); 
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
        optionsPanel.SetActive(true); 
    }

    // --- AYARLAR ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}