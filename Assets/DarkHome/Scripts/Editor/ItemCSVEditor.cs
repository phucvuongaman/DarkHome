#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DarkHome
{
    /// <summary>
    /// ITEM CSV EDITOR - Import/Export Items từ/ra CSV
    /// 
    /// MENU: Tools > DarkHome > CSV Tools > Item Manager
    /// </summary>
    public class ItemCSVEditor : EditorWindow
    {
        private string _csvPath = "Assets/DarkHome/Editor/CSVData/Items.csv";
        private string _soFolder = "Assets/DarkHome/SO/Resources/Chapter1/Items";
        private Vector2 _scrollPos;

        [MenuItem("Tools/DarkHome/CSV Tools/Item Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<ItemCSVEditor>("Item CSV Manager");
            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("📦 ITEM CSV MANAGER", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // ===== EXPORT SECTION =====
            DrawSection("📤 EXPORT Items → CSV", () =>
            {
                EditorGUILayout.HelpBox(
                    "Extract tất cả ItemDataSO từ thư mục SO và export ra file CSV.\n" +
                    "Dùng để backup hoặc chỉnh sửa hàng loạt trong Excel.",
                    MessageType.Info
                );

                _soFolder = EditorGUILayout.TextField("SO Folder:", _soFolder);
                _csvPath = EditorGUILayout.TextField("Output CSV:", _csvPath);

                if (GUILayout.Button("🔽 Export to CSV", GUILayout.Height(30)))
                {
                    ExportItemsToCSV();
                }
            });

            GUILayout.Space(20);

            // ===== IMPORT SECTION =====
            DrawSection("📥 IMPORT CSV → Items", () =>
            {
                EditorGUILayout.HelpBox(
                    "Đọc file CSV và tạo/update ItemDataSO.\n" +
                    "⚠️ LƯU Ý: Backup project trước khi import!",
                    MessageType.Warning
                );

                _csvPath = EditorGUILayout.TextField("Input CSV:", _csvPath);
                _soFolder = EditorGUILayout.TextField("Target SO Folder:", _soFolder);

                if (GUILayout.Button("🔼 Import from CSV", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Confirm Import",
                        "Import sẽ thay đổi/tạo mới SO files. Tiếp tục?",
                        "Import", "Cancel"))
                    {
                        ImportItemsFromCSV();
                    }
                }
            });

            GUILayout.Space(20);

            // ===== LOCALIZATION EXTRACT =====
            DrawSection("🌐 EXTRACT Localization Texts", () =>
            {
                EditorGUILayout.HelpBox(
                    "Tạo file CSV chỉ chứa text cần dịch (itemName, description).\n" +
                    "Gửi cho Translator để họ dịch dễ hơn.",
                    MessageType.Info
                );

                if (GUILayout.Button("📝 Extract Text Only", GUILayout.Height(30)))
                {
                    ExtractLocalizationText();
                }
            });

            EditorGUILayout.EndScrollView();
        }

        // ==================== EXPORT ====================
        private void ExportItemsToCSV()
        {
            // Find all ItemDataSO in folder
            string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { _soFolder });
            var items = new List<ItemDataSO>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item != null) items.Add(item);
            }

            if (items.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Không tìm thấy ItemDataSO nào!", "OK");
                return;
            }

            // Ensure directory exists
            string directory = Path.GetDirectoryName(_csvPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Export using CSVSerializer
            CSVSerializer.ExportToCSV(items, _csvPath);
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Exported {items.Count} items to:\n{_csvPath}", "OK");
        }

        // ==================== IMPORT ====================
        private void ImportItemsFromCSV()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", $"File không tồn tại:\n{_csvPath}", "OK");
                return;
            }

            // Ensure SO folder exists
            if (!AssetDatabase.IsValidFolder(_soFolder))
            {
                Debug.LogError($"SO folder không tồn tại: {_soFolder}");
                return;
            }

            var imported = CSVSerializer.ImportFromCSV<ItemDataSO>(_csvPath, _soFolder);
            
            if (imported != null)
            {
                EditorUtility.DisplayDialog("Success", $"Imported {imported.Count} items!", "OK");
            }
        }

        // ==================== LOCALIZATION ====================
        private void ExtractLocalizationText()
        {
            // Find all items
            string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { _soFolder });
            var locData = new System.Text.StringBuilder();

            locData.AppendLine("Key,EN,VN");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item == null) continue;

                // Generate key from itemID
                string keyPrefix = $"ITEM_{item.itemID}";
                
                locData.AppendLine($"{keyPrefix}_name,\"{item.itemName}\",\"\"");
                locData.AppendLine($"{keyPrefix}_desc,\"{item.description}\",\"\"");
            }

            // Save to Localization folder
            string locPath = "Assets/StreamingAssets/Localization/Items_EN_Template.csv";
            string dir = Path.GetDirectoryName(locPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(locPath, locData.ToString());
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", 
                "Localization template created!\n\n" +
                "Sao chép file này thành Items_VN.csv rồi điền cột VN.",
                "OK");
        }

        // ==================== UI HELPERS ====================
        private void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Space(5);
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
