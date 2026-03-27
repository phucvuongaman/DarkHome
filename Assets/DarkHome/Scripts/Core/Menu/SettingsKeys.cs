using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Centralized constants for all Settings PlayerPrefs keys
    /// Prevents typos and makes refactoring easier
    /// </summary>
    public static class SettingsKeys
    {
        // === AUDIO ===
        public const string MUSIC_VOLUME = "MusicVolume";
        public const string SFX_VOLUME = "SFXVolume";

        // === VIDEO ===
        public const string BRIGHTNESS = "Brightness";
        public const string FULLSCREEN = "Fullscreen";
        public const string QUALITY_LEVEL = "QualityLevel";
        public const string VSYNC = "VSync";

        // === GAME ===
        public const string SHOW_HUD = "ShowHUD";
        public const string SUBTITLES = "Subtitles";

        // === CONTROLS ===
        public const string MOUSE_SENSITIVITY_X = "XSensitivity";
        public const string MOUSE_SENSITIVITY_Y = "YSensitivity";
        public const string MOUSE_SMOOTHING = "MouseSmoothing";
        public const string INVERT_MOUSE = "Inverted";

        // === DEFAULT VALUES ===
        public static class Defaults
        {
            public const float MUSIC_VOLUME = 1.0f;
            public const float SFX_VOLUME = 1.0f;
            public const float BRIGHTNESS = 0.0f;
            public const float MOUSE_SENSITIVITY_X = 2.0f;
            public const float MOUSE_SENSITIVITY_Y = 2.0f;
            public const float MOUSE_SMOOTHING = 0.0f;
            public const int SHOW_HUD = 1; // 1 = on
            public const int SUBTITLES = 1; // 1 = on
            public const int QUALITY_LEVEL = 2; // High quality
        }
    }
}
