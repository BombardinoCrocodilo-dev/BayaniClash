using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip sceneGroupMusic;   // Music for scenes 1–3
    [SerializeField] private AudioClip scene4Music;       // Music for scene 4
    [Range(0f, 1f)] public float musicVolume = 1f;
    [SerializeField] private Slider musicSlider; // assign only in MainScene

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [SerializeField] private Slider sfxSlider; // assign in MainScene

    [Header("Voice")]
    [SerializeField] private AudioSource voiceSource;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [SerializeField] private Slider voiceSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            LoadVolumeSettings();
            // Initialize sliders if they exist
            if (musicSlider != null)
            {
                musicSlider.value = musicVolume;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVolume;
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            PlayMusic(sceneGroupMusic);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Music switching based on scene
        if (scene.name == "BattleScene")
        {
            if (musicSource.clip != scene4Music)
                PlayMusic(scene4Music);
        }
        else
        {
            if (musicSource.clip != sceneGroupMusic)
                PlayMusic(sceneGroupMusic);
        }

        if (scene.name == "MainMenu")
        {
            // Find sliders by tag
            GameObject musicObj = GameObject.FindGameObjectWithTag("MusicSlider");
            GameObject sfxObj = GameObject.FindGameObjectWithTag("SFXSlider");
            GameObject voiceObj = GameObject.FindGameObjectWithTag("VoiceSlider");

            // Assign and setup sliders
            if (musicObj != null)
            {
                musicSlider = musicObj.GetComponent<Slider>();
                SetupSlider(musicSlider, musicVolume, SetMusicVolume);
            }

            if (sfxObj != null)
            {
                sfxSlider = sfxObj.GetComponent<Slider>();
                SetupSlider(sfxSlider, sfxVolume, SetSFXVolume);
            }

            if (voiceObj != null)
            {
                voiceSlider = voiceObj.GetComponent<Slider>();
                SetupSlider(voiceSlider, voiceVolume, SetVoiceVolume);
            }
        }
    }
    private void SetupSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider != null)
        {
            slider.value = value;
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener(callback);
        }
    }

    public void ConnectSliders()
    {
        // Find active sliders
        musicSlider = GameObject.FindGameObjectWithTag("MusicSlider")?.GetComponent<Slider>();
        sfxSlider = GameObject.FindGameObjectWithTag("SFXSlider")?.GetComponent<Slider>();
        voiceSlider = GameObject.FindGameObjectWithTag("VoiceSlider")?.GetComponent<Slider>();

        // Setup sliders if found
        SetupSlider(musicSlider, musicVolume, SetMusicVolume);
        SetupSlider(sfxSlider, sfxVolume, SetSFXVolume);
        SetupSlider(voiceSlider, voiceVolume, SetVoiceVolume);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = musicVolume;
        SaveVolumeSettings();
    }
    public void PlayClick()
    {
        sfxSource.PlayOneShot(clickSound, sfxVolume);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        SaveVolumeSettings();
    }
    public void PlayVoiceLine(AudioClip clip)
    {
        if (clip == null) return;
        voiceSource.PlayOneShot(clip, voiceVolume);
    }
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = volume;
        SaveVolumeSettings();
    }
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        PlayerPrefs.Save(); // Write to disk
    }

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
    }
}
