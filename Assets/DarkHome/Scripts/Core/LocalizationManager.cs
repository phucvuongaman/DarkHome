using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace DarkHome
{
    /// <summary>
    /// LOCALIZATION MANAGER - Hệ thống đa ngôn ngữ cho DarkHome
    /// 
    /// CÁCH HOẠT ĐỘNG:
    /// 1. Load CSV files từ StreamingAssets/Localization/
    /// 2. Parse thành Dictionary<Key, Text>
    /// 3. Cung cấp API GetText(key) để UI/Scripts lấy text
    /// 
    /// CSV FORMAT:
    /// Key,Text
    /// ITEM_MILK_name,Fresh Milk
    /// ITEM_MILK_desc,A bottle of fresh milk...
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private SystemLanguage _defaultLanguage = SystemLanguage.English;

        // Runtime data
        private SystemLanguage _currentLanguage;
        private Dictionary<string, string> _localizedText = new Dictionary<string, string>();

        // Path constants
        private const string LOCALIZATION_FOLDER = "Localization";
        private const string PREF_KEY_LANGUAGE = "GameLanguage";

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            // DontDestroyOnLoad(gameObject);

            // Auto-load language based on system (hoặc PlayerPrefs)
            _currentLanguage = GetSavedLanguage();
            LoadLanguage(_currentLanguage);
        }

        /// <summary>
        /// Lấy text theo key. Nếu không tìm thấy, trả về key để dễ debug.
        /// </summary>
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key)) return "---";
            if (_localizedText.TryGetValue(key, out string value))
            {
                return value;
            }

            Debug.LogWarning($"[Localization] Missing key: {key} for language: {_currentLanguage}");
            return $"[{key}]"; // Hiển thị key để Designer biết chỗ nào thiếu
        }

        /// <summary>
        /// Chuyển ngôn ngữ runtime (VD: Player bấm nút Settings)
        /// </summary>
        public void ChangeLanguage(SystemLanguage newLanguage)
        {
            if (_currentLanguage == newLanguage) return;

            _currentLanguage = newLanguage;
            LoadLanguage(newLanguage);
            SaveLanguagePreference(newLanguage);

            // Broadcast event để UI refresh
            EventManager.Notify(GameEvents.Localization.OnLanguageChanged, newLanguage);
            Debug.Log($"🌐 Language changed to: {newLanguage}");
        }

        public SystemLanguage CurrentLanguage => _currentLanguage;

        // ==================== PRIVATE METHODS ====================

        private void LoadLanguage(SystemLanguage language)
        {
            _localizedText.Clear();

            // Xác định tên file CSV
            string fileName = GetLanguageFileName(language);
            string filePath = Path.Combine(Application.streamingAssetsPath, LOCALIZATION_FOLDER, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"[Localization] File not found: {filePath}");

                // Fallback to default language (only if not already trying default)
                if (language != _defaultLanguage)
                {
                    Debug.LogWarning($"[Localization] Falling back to {_defaultLanguage}");
                    LoadLanguage(_defaultLanguage);
                }
                else
                {
                    // SAFETY: Default language file is also missing - stop recursion
                    Debug.LogError($"[Localization] CRITICAL: Default language file missing! Game will use fallback keys.");
                }
                return;
            }

            // Parse CSV
            try
            {
                string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

                // Skip header (line 0)
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Split by first comma only (vì text có thể chứa dấu phẩy)
                    int firstComma = line.IndexOf(',');
                    if (firstComma == -1) continue;

                    string key = line.Substring(0, firstComma).Trim();
                    string value = line.Substring(firstComma + 1).Trim();

                    // Remove quotes nếu có (Excel thường thêm quotes cho text có dấu phẩy)
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    _localizedText[key] = value;
                }

                Debug.Log($"✅ Loaded {_localizedText.Count} localization entries for {language}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Localization] Failed to parse CSV: {e.Message}");
            }
        }

        private string GetLanguageFileName(SystemLanguage language)
        {
            // Map SystemLanguage to file names
            switch (language)
            {
                case SystemLanguage.English:
                    return "EN.csv";
                case SystemLanguage.Vietnamese:
                    return "VN.csv";
                // Thêm ngôn ngữ khác ở đây
                default:
                    return "EN.csv"; // Default fallback
            }
        }

        private SystemLanguage GetSavedLanguage()
        {
            // Load từ PlayerPrefs (hoặc Save System của game)
            string savedLang = PlayerPrefs.GetString(PREF_KEY_LANGUAGE, "");

            if (string.IsNullOrEmpty(savedLang))
            {
                // Lần đầu chơi: dùng ngôn ngữ hệ thống
                return Application.systemLanguage == SystemLanguage.Vietnamese
                    ? SystemLanguage.Vietnamese
                    : SystemLanguage.English;
            }

            // Parse saved language
            return System.Enum.TryParse(savedLang, out SystemLanguage lang)
                ? lang
                : _defaultLanguage;
        }

        private void SaveLanguagePreference(SystemLanguage language)
        {
            PlayerPrefs.SetString(PREF_KEY_LANGUAGE, language.ToString());
            PlayerPrefs.Save();
        }

        // ==================== EDITOR UTILITIES ====================

#if UNITY_EDITOR
        /// <summary>
        /// [EDITOR ONLY] Lấy tất cả keys hiện có để validate
        /// </summary>
        public List<string> GetAllKeys()
        {
            return _localizedText.Keys.ToList();
        }
#endif
    }
}
