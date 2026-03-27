using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DarkHome
{
    /// <summary>
    /// Controller cho TAB VIDEO trong Settings Menu
    /// Quản lý: Fullscreen, Quality Presets, Brightness
    /// </summary>
    public class VideoSettingsController : MonoBehaviour
    {
        [Header("UI References - Toggles")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Text _fullscreenText;

        [Header("UI References - Quality Buttons")]
        [Tooltip("Buttons cho Low/Medium/High quality")]
        [SerializeField] private Button _qualityLowButton;
        [SerializeField] private Button _qualityMediumButton;
        [SerializeField] private Button _qualityHighButton;

        [Header("UI References - Highlight Lines")]
        [Tooltip("Visual indicators cho button nào đang active")]
        [SerializeField] private GameObject _qualityLowHighlight;
        [SerializeField] private GameObject _qualityMedHighlight;
        [SerializeField] private GameObject _qualityHighHighlight;

        [Header("UI References - Sliders")]
        [SerializeField] private Slider _brightnessSlider;

        private void Start()
        {
            RegisterListeners(); // Chỉ register 1 lần
        }

        private void OnEnable()
        {
            // Reload settings MỖI KHI panel được enable
            // Đảm bảo sync giữa Main Menu (3D) và In-Game (2D)
            LoadSettings();
        }

        private void LoadSettings()
        {
            // FULLSCREEN
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.isOn = Screen.fullScreen;
                UpdateToggleText(_fullscreenText, Screen.fullScreen);
            }

            // QUALITY LEVEL
            int quality = PlayerPrefs.GetInt(SettingsKeys.QUALITY_LEVEL, SettingsKeys.Defaults.QUALITY_LEVEL);
            QualitySettings.SetQualityLevel(quality);
            UpdateQualityHighlight(quality);

            // BRIGHTNESS
            if (_brightnessSlider != null)
            {
                float brightness = PlayerPrefs.GetFloat(SettingsKeys.BRIGHTNESS, SettingsKeys.Defaults.BRIGHTNESS);
                _brightnessSlider.value = brightness;
            }
        }

        private void RegisterListeners()
        {
            // Fullscreen
            if (_fullscreenToggle != null)
                _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            // Quality buttons
            if (_qualityLowButton != null)
                _qualityLowButton.onClick.AddListener(() => SetQuality(0));

            if (_qualityMediumButton != null)
                _qualityMediumButton.onClick.AddListener(() => SetQuality(1));

            if (_qualityHighButton != null)
                _qualityHighButton.onClick.AddListener(() => SetQuality(2));

            // Brightness
            if (_brightnessSlider != null)
                _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        #region === Callback Functions ===

        public void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            PlayerPrefs.SetInt(SettingsKeys.FULLSCREEN, isFullscreen ? 1 : 0);
            PlayerPrefs.Save();

            UpdateToggleText(_fullscreenText, isFullscreen);

            // Debug.Log($"🖥️ Fullscreen: {(isFullscreen ? "ON" : "OFF")}");
        }

        public void SetQuality(int qualityLevel)
        {
            // Clamp to valid range (0 = Low, 1 = Medium, 2 = High)
            qualityLevel = Mathf.Clamp(qualityLevel, 0, 2);

            QualitySettings.SetQualityLevel(qualityLevel);
            PlayerPrefs.SetInt(SettingsKeys.QUALITY_LEVEL, qualityLevel);
            PlayerPrefs.Save();

            UpdateQualityHighlight(qualityLevel);

            string qualityName = qualityLevel == 0 ? "Low" : qualityLevel == 1 ? "Medium" : "High";
            // Debug.Log($"⚙️ Quality: {qualityName}");
        }

        public void OnBrightnessChanged(float value)
        {
            // Gửi đến VolumeManager để apply Post Processing
            if (VolumeManager.Instance != null)
                VolumeManager.Instance.SetBrightness(value);
        }

        #endregion

        #region === Helper Functions ===

        private void UpdateToggleText(TMP_Text textComponent, bool isOn)
        {
            if (textComponent != null)
            {
                textComponent.text = isOn ? "on" : "off";
            }
        }

        private void UpdateQualityHighlight(int activeQuality)
        {
            // Tắt hết highlights
            if (_qualityLowHighlight != null) _qualityLowHighlight.SetActive(false);
            if (_qualityMedHighlight != null) _qualityMedHighlight.SetActive(false);
            if (_qualityHighHighlight != null) _qualityHighHighlight.SetActive(false);

            // Bật highlight cho quality đang active
            switch (activeQuality)
            {
                case 0:
                    if (_qualityLowHighlight != null) _qualityLowHighlight.SetActive(true);
                    break;
                case 1:
                    if (_qualityMedHighlight != null) _qualityMedHighlight.SetActive(true);
                    break;
                case 2:
                    if (_qualityHighHighlight != null) _qualityHighHighlight.SetActive(true);
                    break;
            }
        }

        #endregion
    }
}
