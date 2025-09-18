using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    [SerializeField] private GameObject settingMenu;
    [Header("Buttons")]
    [SerializeField] private Button musicToggleButton;
    [SerializeField] private Button sfxToggleButton;
    [SerializeField] private Button backButton;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Icons")]
    [SerializeField] private Sprite musicOnIcon;
    [SerializeField] private Sprite musicOffIcon;
    [SerializeField] private Sprite sfxOnIcon;
    [SerializeField] private Sprite sfxOffIcon;

    private void Start()
    {
        RegisterEvents();
        InitializeUI();
        settingMenu.SetActive(false);
    }

    private void RegisterEvents()
    {
        musicSlider.onValueChanged.AddListener(HandleMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(HandleSFXSliderChanged);

        musicToggleButton.onClick.AddListener(HandleMusicToggle);
        sfxToggleButton.onClick.AddListener(HandleSFXToggle);

        backButton.onClick.AddListener(OnHide);
    }

    private void InitializeUI()
    {
        musicSlider.value = SoundManager.Instance.GetCurrentMusicVolume();
        sfxSlider.value = SoundManager.Instance.GetCurrentVfxVolume();

        UpdateAudioVolumes();
        UpdateIcons();
    }

    private void HandleMusicSliderChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
        AudioManager.Instance.GetMusicAudioSource().volume = value;
        UpdateIcons();
    }

    private void HandleSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        AudioManager.Instance.GetVFXAudioSource().volume = value;
        UpdateIcons();
    }

    private void HandleMusicToggle()
    {
        SoundManager.Instance.ToggleMusicMute();
        float newVolume = SoundManager.Instance.GetCurrentMusicVolume();
        musicSlider.value = newVolume;
        AudioManager.Instance.GetMusicAudioSource().volume = newVolume;
        AudioManager.Instance.PlayVFX("Choose");
        UpdateIcons();
    }

    private void HandleSFXToggle()
    {
        SoundManager.Instance.ToggleSFXMute();
        float newVolume = SoundManager.Instance.GetCurrentVfxVolume();
        sfxSlider.value = newVolume;
        AudioManager.Instance.GetVFXAudioSource().volume = newVolume;
        AudioManager.Instance.PlayVFX("Choose");
        UpdateIcons();
    }

    private void UpdateAudioVolumes()
    {
        AudioManager.Instance.GetMusicAudioSource().volume = SoundManager.Instance.GetCurrentMusicVolume();
        AudioManager.Instance.GetVFXAudioSource().volume = SoundManager.Instance.GetCurrentVfxVolume();
    }

    private void UpdateIcons()
    {
        musicToggleButton.image.sprite = SoundManager.Instance.SoundData.IsMusicMuted ? musicOffIcon : musicOnIcon;
        sfxToggleButton.image.sprite = SoundManager.Instance.SoundData.IsSFXMuted ? sfxOffIcon : sfxOnIcon;
    }

    private void OnHide()
    {
        // Ẩn hoặc đóng menu setting nếu cần
        AudioManager.Instance.PlayVFX("Choose");
        settingMenu.SetActive(false);
    }
}
