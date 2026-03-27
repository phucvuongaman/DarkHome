using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DarkHome
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private AudioMixer _audioMixer;
        // [SerializeField] private Volume _globalPostProcessingVolume;

        // private ColorAdjustments _colorAdjust;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            // if (_globalPostProcessingVolume != null) { _globalPostProcessingVolume.profile.TryGet(out _colorAdjust); }

            LoadSettings(); // Tải cài đặt ngay khi game bắt đầu
        }

        private void LoadSettings()
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
            SetSfxVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
            // SetBrightness(PlayerPrefs.GetFloat("Brightness", 0f));
        }

        // --- CÁC HÀM API "CHUẨN" ĐỂ THAY ĐỔI SETTING ---
        public void SetMusicVolume(float value)
        {
            float volumeDb = (value > 0.001f) ? Mathf.Log10(value) * 20 : -80f;
            _audioMixer.SetFloat("MusicVolume", volumeDb);
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void SetSfxVolume(float value)
        {
            float volumeDb = (value > 0.001f) ? Mathf.Log10(value) * 20 : -80f;
            _audioMixer.SetFloat("SFXVolume", volumeDb);
            PlayerPrefs.SetFloat("SFXVolume", value);
        }


    }
}