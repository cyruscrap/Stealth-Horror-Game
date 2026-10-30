using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityTutorial.PlayerControl;
using Zenject;

public class OptionsMenu : MonoBehaviour
{
    [Inject] WorldSaveGameManager _worldSaveGameManager;
    [InjectOptional] private PlayerController _playerController;
    [InjectOptional] private PlayerInteractionManager _playerInteractionManager;
    [InjectOptional] private PostProcessingManager _postProcessingManager;
    [InjectOptional] private CutsceneManager _cutsceneManager;

    [SerializeField] AudioMixer audioMixer;

    [Header("Input & Camera")]
    [SerializeField] Slider mouseSensivity;
    [SerializeField] UnityEngine.UI.Toggle invertXAxis;
    [SerializeField] UnityEngine.UI.Toggle invertYAxis;

    [Header("Gameplay")]
    [SerializeField] UnityEngine.UI.Toggle subtitles;
    [SerializeField] UnityEngine.UI.Toggle interactableHighlights;
    [SerializeField] Slider readingSpeed;

    [Header("Graphics & Display")]
    [SerializeField] UnityEngine.UI.Toggle cameraShake;
    [SerializeField] UnityEngine.UI.Toggle motionBlur;
    [SerializeField] Slider gamma;

    [Header("Audio")]
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;
    [SerializeField] Slider voiceVolume;

    private void OnLevelWasLoaded()
    {
        PlayerPrefs.SetInt("UnitySelectMonitor", 0);

        if (_worldSaveGameManager.CheckIfSaveFileExists() || PlayerPrefs.HasKey("IsInitialized"))
        {
            PlayerPrefs.SetInt("IsInitialized", 1);

            mouseSensivity.value = PlayerPrefs.GetFloat("MouseSensivity", mouseSensivity.maxValue / 2);
            invertXAxis.isOn = PlayerPrefsExtentions.GetBool("InvertXAxis");
            invertYAxis.isOn = PlayerPrefsExtentions.GetBool("InvertYAxis");

            subtitles.isOn = PlayerPrefsExtentions.GetBool("Subtitles", true);
            interactableHighlights.isOn = PlayerPrefsExtentions.GetBool("InteractableHighlights", true);
            readingSpeed.value = PlayerPrefs.GetFloat("ReadingSpeed");
            
            cameraShake.isOn = PlayerPrefsExtentions.GetBool("CameraShake", true);
            motionBlur.isOn = PlayerPrefsExtentions.GetBool("MotionBlur");
            gamma.value = PlayerPrefs.GetFloat("Gamma");

            musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
            sfxVolume.value = PlayerPrefs.GetFloat("SFXVolume");
            voiceVolume.value = PlayerPrefs.GetFloat("VoiceVolume");
        }
        else
        {
            // Default Values

            mouseSensivity.value = mouseSensivity.maxValue / 2;
            PlayerPrefs.SetFloat("MouseSensivity", mouseSensivity.value);

            invertXAxis.isOn = false;
            PlayerPrefsExtentions.SetBool("InvertXAxis", invertXAxis.isOn);

            invertYAxis.isOn = false;
            PlayerPrefsExtentions.SetBool("InvertYAxis", invertYAxis.isOn);

            subtitles.isOn = true;
            PlayerPrefsExtentions.SetBool("Subtitles", subtitles.isOn);

            interactableHighlights.isOn = true;
            PlayerPrefsExtentions.SetBool("InteractableHighlights", interactableHighlights.isOn);

            readingSpeed.value = readingSpeed.maxValue / 2;
            PlayerPrefs.SetFloat("ReadingSpeed", readingSpeed.value);

            cameraShake.isOn = true;
            PlayerPrefsExtentions.SetBool("CameraShake", cameraShake.isOn);

            motionBlur.isOn = false;
            PlayerPrefsExtentions.SetBool("MotionBlur", motionBlur.isOn);

            gamma.value = -0.1f;
            PlayerPrefs.SetFloat("Gamma", gamma.value);

            musicVolume.value = musicVolume.maxValue / 2;
            PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);

            sfxVolume.value = sfxVolume.maxValue / 2;
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume.value);

            voiceVolume.value = voiceVolume.maxValue / 2;
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume.value);
        }

        ApplyOptionsToGameSystems();
    }

    public void ApplyOptionsToGameSystems()
    {
        if (_playerController != null)
        {
            _playerController.MouseSensitivity = mouseSensivity.value;
            _playerController.InvertXAxis = invertXAxis.isOn;
            _playerController.InvertYAxis = invertYAxis.isOn;
            _playerController.CameraShake = cameraShake.isOn;
        }

        if (_playerInteractionManager != null) _playerInteractionManager.showOutline = interactableHighlights.isOn;

        if (_cutsceneManager != null) _cutsceneManager.SwitchSubtitles(subtitles.isOn);

        if (_postProcessingManager != null) 
        {
            _postProcessingManager.SwitchMotionBlur(motionBlur.isOn);
            _postProcessingManager.SetGamma(gamma.value);
        }

        if (DialogueManager.Instance != null) DialogueManager.DisplaySettings.subtitleSettings.subtitleCharsPerSecond = readingSpeed.value;

        audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume.value) * 20);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume.value) * 20);
        audioMixer.SetFloat("VoiceVolume", Mathf.Log10(voiceVolume.value) * 20);
    }

    // Input & Camera

    public void UpdateMouseSensivity()
    {
        PlayerPrefs.SetFloat("MouseSensivity", mouseSensivity.value);
        if (_playerController != null) _playerController.MouseSensitivity = mouseSensivity.value;
    }

    public void InvertXAxis()
    {
        PlayerPrefsExtentions.SetBool("InvertXAxis", invertXAxis.isOn);
        if (_playerController != null) _playerController.InvertXAxis = invertXAxis.isOn;
    }

    public void InvertYAxis()
    {
        PlayerPrefsExtentions.SetBool("InvertYAxis", invertYAxis.isOn);
        if (_playerController != null) _playerController.InvertYAxis = invertYAxis.isOn;
    }

    // Gameplay

    public void SwitchSubtitles()
    {
        PlayerPrefsExtentions.SetBool("Subtitles", subtitles.isOn);
        if (_cutsceneManager != null) _cutsceneManager.SwitchSubtitles(subtitles.isOn);
    }

    public void SwitchInteractableHighlights()
    {
        PlayerPrefsExtentions.SetBool("InteractableHighlights", interactableHighlights.isOn);
        if (_playerInteractionManager != null) _playerInteractionManager.showOutline = interactableHighlights.isOn;
    }

    public void UpdateReadingSpeed()
    {
        PlayerPrefs.SetFloat("ReadingSpeed", readingSpeed.value);
        if (DialogueManager.Instance != null) DialogueManager.DisplaySettings.subtitleSettings.subtitleCharsPerSecond = readingSpeed.value;
    }

    // Graphics & Display

    public void SwitchCameraShake()
    {
        PlayerPrefsExtentions.SetBool("CameraShake", cameraShake.isOn);
        if (_playerController != null) _playerController.CameraShake = cameraShake.isOn;
    }

    public void SwitchMotionBlur()
    {
        PlayerPrefsExtentions.SetBool("MotionBlur", motionBlur.isOn);

        /*if (_postProcessingManager == null)
        {
            Debug.LogError("[UI]  –»“»◊≈— ¿ﬂ Œÿ»¡ ¿: _postProcessingManager ‡‚ÂÌ NULL!");
            return;
        }*/

        if (_postProcessingManager != null) _postProcessingManager.SwitchMotionBlur(motionBlur.isOn);
    }

    public void UpdateGamma()
    {
        PlayerPrefs.SetFloat("Gamma", gamma.value);
        if (_postProcessingManager != null) _postProcessingManager.SetGamma(gamma.value);
    }

    //Audio

    public void UpdateMusicVolume()
    {
        float volume = musicVolume.value;
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
    }

    public void UpdateSFXVolume()
    {
        float volume = sfxVolume.value;
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void UpdateVoiceVolume()
    {
        float volume = voiceVolume.value;
        audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("VoiceVolume", volume);
    }
}