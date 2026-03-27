using UnityEngine;
using UnityEngine.UI;

namespace DarkHome
{
    /// <summary>
    /// Controller cho TAB CONTROLS trong Settings Menu
    /// Quản lý: Mouse Sensitivity X/Y, Mouse Smoothing
    /// </summary>
    public class ControlsSettingsController : MonoBehaviour
    {
        [Header("UI References - Sliders")]
        [SerializeField] private Slider _sensitivityXSlider;
        [SerializeField] private Slider _sensitivityYSlider;
        [SerializeField] private Slider _mouseSmoothingSlider;

        [Header("Slider Ranges")]
        [Tooltip("Range cho Mouse Sensitivity")]
        [SerializeField] private Vector2 _sensitivityRange = new Vector2(0.5f, 5.0f);
        [Tooltip("Range cho Mouse Smoothing")]
        [SerializeField] private Vector2 _smoothingRange = new Vector2(0f, 10f);

        private void Start()
        {
            SetupSliderRanges();
            RegisterListeners(); // Chỉ register 1 lần
        }

        private void OnEnable()
        {
            // Reload settings MỖI KHI panel được enable
            // Đảm bảo sync giữa Main Menu (3D) và In-Game (2D)
            LoadSettings();
        }

        private void SetupSliderRanges()
        {
            // Set slider min/max values
            if (_sensitivityXSlider != null)
            {
                _sensitivityXSlider.minValue = _sensitivityRange.x;
                _sensitivityXSlider.maxValue = _sensitivityRange.y;
            }

            if (_sensitivityYSlider != null)
            {
                _sensitivityYSlider.minValue = _sensitivityRange.x;
                _sensitivityYSlider.maxValue = _sensitivityRange.y;
            }

            if (_mouseSmoothingSlider != null)
            {
                _mouseSmoothingSlider.minValue = _smoothingRange.x;
                _mouseSmoothingSlider.maxValue = _smoothingRange.y;
            }
        }

        private void LoadSettings()
        {
            // Load từ PlayerPrefs
            if (_sensitivityXSlider != null)
            {
                float sensX = PlayerPrefs.GetFloat(SettingsKeys.MOUSE_SENSITIVITY_X, SettingsKeys.Defaults.MOUSE_SENSITIVITY_X);
                _sensitivityXSlider.value = sensX;
            }

            if (_sensitivityYSlider != null)
            {
                float sensY = PlayerPrefs.GetFloat(SettingsKeys.MOUSE_SENSITIVITY_Y, SettingsKeys.Defaults.MOUSE_SENSITIVITY_Y);
                _sensitivityYSlider.value = sensY;
            }

            if (_mouseSmoothingSlider != null)
            {
                float smoothing = PlayerPrefs.GetFloat(SettingsKeys.MOUSE_SMOOTHING, SettingsKeys.Defaults.MOUSE_SMOOTHING);
                _mouseSmoothingSlider.value = smoothing;
            }
        }

        private void RegisterListeners()
        {
            if (_sensitivityXSlider != null)
                _sensitivityXSlider.onValueChanged.AddListener(OnSensitivityXChanged);

            if (_sensitivityYSlider != null)
                _sensitivityYSlider.onValueChanged.AddListener(OnSensitivityYChanged);

            if (_mouseSmoothingSlider != null)
                _mouseSmoothingSlider.onValueChanged.AddListener(OnMouseSmoothingChanged);
        }

        #region === Callback Functions ===

        public void OnSensitivityXChanged(float value)
        {
            PlayerPrefs.SetFloat(SettingsKeys.MOUSE_SENSITIVITY_X, value);
            PlayerPrefs.Save();

            // Nếu đang in-game, apply ngay
            // (FirstPersonController hoặc PlayerMouseLook sẽ đọc từ PlayerPrefs)
            Debug.Log($"🖱️ Mouse Sensitivity X: {value:F2}");
        }

        public void OnSensitivityYChanged(float value)
        {
            PlayerPrefs.SetFloat(SettingsKeys.MOUSE_SENSITIVITY_Y, value);
            PlayerPrefs.Save();

            Debug.Log($"🖱️ Mouse Sensitivity Y: {value:F2}");
        }

        public void OnMouseSmoothingChanged(float value)
        {
            PlayerPrefs.SetFloat(SettingsKeys.MOUSE_SMOOTHING, value);
            PlayerPrefs.Save();

            Debug.Log($"🖱️ Mouse Smoothing: {value:F2}");
        }

        #endregion

        #region === Public API ===

        /// <summary>
        /// Reset controls settings về default
        /// Có thể gọi từ "Reset to Default" button
        /// </summary>
        public void ResetToDefaults()
        {
            if (_sensitivityXSlider != null)
                _sensitivityXSlider.value = SettingsKeys.Defaults.MOUSE_SENSITIVITY_X;

            if (_sensitivityYSlider != null)
                _sensitivityYSlider.value = SettingsKeys.Defaults.MOUSE_SENSITIVITY_Y;

            if (_mouseSmoothingSlider != null)
                _mouseSmoothingSlider.value = SettingsKeys.Defaults.MOUSE_SMOOTHING;

            Debug.Log("🔄 Controls settings reset to defaults");
        }

        #endregion
    }
}
