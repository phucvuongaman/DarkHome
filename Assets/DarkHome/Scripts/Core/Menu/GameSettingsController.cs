using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DarkHome
{
    /// <summary>
    /// Controller cho TAB GAME trong Settings Menu
    /// Quản lý: Music, Sound, Show HUD, Subtitles
    /// </summary>
    public class GameSettingsController : MonoBehaviour
    {
        [Header("UI References - Sliders")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundSlider;

        [Header("UI References - Toggles")]
        [SerializeField] private Toggle _showHudToggle;
        [SerializeField] private Toggle _subtitlesToggle;

        [Header("UI References - Text (Optional)")]
        [Tooltip("Text hiển thị 'on/off' bên cạnh toggle")]
        [SerializeField] private TMP_Text _showHudText;
        [SerializeField] private TMP_Text _subtitlesText;

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
            // Load từ PlayerPrefs và apply lên UI
            if (_musicSlider != null)
            {
                float music = PlayerPrefs.GetFloat(SettingsKeys.MUSIC_VOLUME, SettingsKeys.Defaults.MUSIC_VOLUME);
                _musicSlider.value = music;
            }

            if (_soundSlider != null)
            {
                float sound = PlayerPrefs.GetFloat(SettingsKeys.SFX_VOLUME, SettingsKeys.Defaults.SFX_VOLUME);
                _soundSlider.value = sound;
            }

            if (_showHudToggle != null)
            {
                int showHud = PlayerPrefs.GetInt(SettingsKeys.SHOW_HUD, SettingsKeys.Defaults.SHOW_HUD);
                _showHudToggle.isOn = (showHud == 1);
                UpdateToggleText(_showHudText, showHud == 1);
            }

            if (_subtitlesToggle != null)
            {
                int subtitles = PlayerPrefs.GetInt(SettingsKeys.SUBTITLES, SettingsKeys.Defaults.SUBTITLES);
                _subtitlesToggle.isOn = (subtitles == 1);
                UpdateToggleText(_subtitlesText, subtitles == 1);
            }
        }

        private void RegisterListeners()
        {
            // Tự động kết nối events thay vì phải kéo thả trong Inspector
            if (_musicSlider != null)
                _musicSlider.onValueChanged.AddListener(OnMusicChanged);

            if (_soundSlider != null)
                _soundSlider.onValueChanged.AddListener(OnSoundChanged);

            if (_showHudToggle != null)
                _showHudToggle.onValueChanged.AddListener(OnShowHudChanged);

            if (_subtitlesToggle != null)
                _subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
        }

        #region === Callback Functions ===

        public void OnMusicChanged(float value)
        {
            // Gửi đến SettingsManager để apply vào AudioMixer
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetMusicVolume(value);
        }

        public void OnSoundChanged(float value)
        {
            // Gửi đến SettingsManager để apply vào AudioMixer
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetSfxVolume(value);
        }

        public void OnShowHudChanged(bool isOn)
        {
            PlayerPrefs.SetInt(SettingsKeys.SHOW_HUD, isOn ? 1 : 0);
            PlayerPrefs.Save();

            UpdateToggleText(_showHudText, isOn);

            // Nếu đang in-game, apply ngay lập tức
            if (UIManager.Instance != null && UIManager.Instance.HudCanvas != null)
            {
                UIManager.Instance.HudCanvas.SetActive(isOn);
            }

            // Debug.Log($"🎮 Show HUD: {(isOn ? "ON" : "OFF")}");
        }

        public void OnSubtitlesChanged(bool isOn)
        {
            PlayerPrefs.SetInt(SettingsKeys.SUBTITLES, isOn ? 1 : 0);
            PlayerPrefs.Save();

            UpdateToggleText(_subtitlesText, isOn);

            // DialogueUI sẽ check PlayerPrefs này khi hiển thị
            // Debug.Log($"💬 Subtitles: {(isOn ? "ON" : "OFF")}");
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

        #endregion
    }
}
