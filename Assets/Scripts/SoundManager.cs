using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Music Clips")]
    public AudioClip menuMusic; // Menü müziði
    public AudioClip gameMusic; // Oyun içi müzik

    [Header("SFX Clips - Character")]
    public AudioClip playerMoveSound;
    public AudioClip playerDeathSound;

    [Header("SFX Clips - Enemy")]
    public AudioClip enemyMoveSound;
    public AudioClip enemyDeathSound;

    [Header("SFX Clips - General")]
    public AudioClip collectibleSound;
    public AudioClip enemyDefeatedSound; // Oyuncu düþmaný kestiðinde

    [Header("Audio Sources")]
    public AudioSource musicSource; // Müzikler için (Loop açýk olmalý)
    public AudioSource sfxSource;   // Efektler için

    void Awake()
    {
        // Singleton Yapýsý (Sahne geçiþlerinde yok olmasýn)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþse bile müzik kesilmez
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // PlayOneShot, üst üste ses çalmaya izin verir (ör: ayný anda 2 düþman ölürse)
        sfxSource.PlayOneShot(clip);
    }

    // Ses ayarý (Slider için opsiyonel)
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}