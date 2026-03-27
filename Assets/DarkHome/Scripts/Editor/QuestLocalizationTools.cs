// Deleted
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace DarkHome
{
    /// <summary>
    /// QUEST LOCALIZATION TOOLS - Export/Import translations for quests
    /// 
    /// Export: Extract all quest localization keys and create translation template
    /// Import: Merge translated CSV back into EN.csv and VN.csv
    /// 
    /// MENU: Tools > DarkHome > Quest Localization
    /// </summary>
    public class QuestLocalizationTools : EditorWindow
    {
        private Vector2 _scrollPos;
        private string _questDataFolder = "Assets/DarkHome/SO/Resources";

        [MenuItem("Tools/DarkHome/Quest Localization")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuestLocalizationTools>("Quest Localization");
            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("🌐 QUEST LOCALIZATION TOOLS", EditorStyles.largeLabel);
            GUILayout.Space(10);

            // Export Section
            DrawExportSection();

            GUILayout.Space(20);

            // Import Section
            DrawImportSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawExportSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📤 EXPORT Quest Texts for Translation", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Extract all quest localization keys from QuestDataSO files.\n\n" +
                "Creates: Quests_Translation_Template.csv\n" +
                "Format: Key, Text_EN, Text_VN (empty)\n\n" +
                "Send this file to translator for Vietnamese translation.",
                MessageType.Info
            );

            _questDataFolder = EditorGUILayout.TextField("Quest SO Folder:", _questDataFolder);

            GUILayout.Space(5);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🔽 Export Translation Template", GUILayout.Height(40)))
            {
                ExportQuestTexts();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawImportSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📥 IMPORT Translated Quest Texts", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Import translated quest texts from template CSV.\n\n" +
                "Reads: Quests_Translation_Template.csv\n" +
                "Updates: EN.csv (EN column) and VN.csv (VN column)\n\n" +
                "⚠️ WARNING: This will modify localization files!",
                MessageType.Warning
            );

            GUILayout.Space(5);

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("🔼 Import Translations", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm Import",
                    "This will update EN.csv and VN.csv.\n\nBackup recommended. Continue?",
                    "Import", "Cancel"))
                {
                    ImportQuestTranslations();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        #region Export Logic

        private void ExportQuestTexts()
        {
            // Find all QuestDataSO files
            string[] guids = AssetDatabase.FindAssets("t:QuestDataSO", new[] { _questDataFolder });

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", $"No QuestDataSO found in '{_questDataFolder}'!", "OK");
                return;
            }

            // Load EN.csv to get existing English text
            string enCsvPath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/EN.csv");
            var enDict = LoadCSVToDictionary(enCsvPath);

            if (enDict.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "EN.csv is empty or not found!", "OK");
                return;
            }

            // Build template CSV
            var sb = new StringBuilder();
            sb.AppendLine("Key,Text_EN,Text_VN");

            int questCount = 0;
            int keyCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var questData = AssetDatabase.LoadAssetAtPath<QuestDataSO>(path);
                if (questData == null) continue;

                foreach (var quest in questData.quests)
                {
                    questCount++;

                    // Quest name
                    if (!string.IsNullOrEmpty(quest.QuestNameKey))
                    {
                        string enText = enDict.ContainsKey(quest.QuestNameKey) ? enDict[quest.QuestNameKey] : "";
                        sb.AppendLine($"{quest.QuestNameKey},\"{EscapeCSV(enText)}\",\"\"");
                        keyCount++;
                    }

                    // Quest description
                    if (!string.IsNullOrEmpty(quest.DescriptionKey))
                    {
                        string enText = enDict.ContainsKey(quest.DescriptionKey) ? enDict[quest.DescriptionKey] : "";
                        sb.AppendLine($"{quest.DescriptionKey},\"{EscapeCSV(enText)}\",\"\"");
                        keyCount++;
                    }

                    // Objectives
                    if (quest.Objectives != null)
                    {
                        foreach (var obj in quest.Objectives)
                        {
                            if (!string.IsNullOrEmpty(obj.DescriptionKey))
                            {
                                string enText = enDict.ContainsKey(obj.DescriptionKey) ? enDict[obj.DescriptionKey] : "";
                                sb.AppendLine($"{obj.DescriptionKey},\"{EscapeCSV(enText)}\",\"\"");
                                keyCount++;
                            }
                        }
                    }
                }
            }

            // Save template
            string outputPath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/Quests_Translation_Template.csv");
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Export Complete!",
                $"Exported {questCount} quests with {keyCount} localization keys!\n\n" +
                $"File: {outputPath}\n\n" +
                "Send this file to translator to fill the Text_VN column.",
                "OK"
            );
        }

        #endregion

        #region Import Logic

        private void ImportQuestTranslations()
        {
            string templatePath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/Quests_Translation_Template.csv");

            if (!File.Exists(templatePath))
            {
                EditorUtility.DisplayDialog("Error", "Translation template not found!\n\nExport it first.", "OK");
                return;
            }

            // Read template
            string[] lines = File.ReadAllLines(templatePath, Encoding.UTF8);

            if (lines.Length < 2)
            {
                EditorUtility.DisplayDialog("Error", "Translation template is empty!", "OK");
                return;
            }

            // Load existing CSVs
            string enCsvPath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/EN.csv");
            string vnCsvPath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/VN.csv");

            var enDict = LoadCSVToDictionary(enCsvPath);
            var vnDict = LoadCSVToDictionary(vnCsvPath);

            // Parse template and merge
            int updatedEN = 0;
            int updatedVN = 0;

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = SplitCSVLine(line);
                if (parts.Length < 3) continue;

                string key = parts[0].Trim();
                string textEN = parts[1].Trim().Trim('\"');
                string textVN = parts[2].Trim().Trim('\"');

                // Update EN if exists
                if (!string.IsNullOrEmpty(textEN))
                {
                    enDict[key] = textEN;
                    updatedEN++;
                }

                // Update VN if exists
                if (!string.IsNullOrEmpty(textVN))
                {
                    vnDict[key] = textVN;
                    updatedVN++;
                }
            }

            // Write back to CSVs
            WriteCSV(enCsvPath, enDict);
            WriteCSV(vnCsvPath, vnDict);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Import Complete!",
                $"Updated localization files!\n\n" +
                $"EN.csv: {updatedEN} keys\n" +
                $"VN.csv: {updatedVN} keys\n\n" +
                "Test language switching in game!",
                "OK"
            );
        }

        #endregion

        #region Helper Methods

        private Dictionary<string, string> LoadCSVToDictionary(string path)
        {
            var dict = new Dictionary<string, string>();

            if (!File.Exists(path))
            {
                Debug.LogWarning($"CSV not found: {path}");
                return dict;
            }

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = SplitCSVLine(line);
                if (parts.Length >= 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim().Trim('\"');
                    dict[key] = value;
                }
            }

            return dict;
        }

        private void WriteCSV(string path, Dictionary<string, string> dict)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Key,Text");

            foreach (var kvp in dict.OrderBy(x => x.Key))
            {
                sb.AppendLine($"{kvp.Key},\"{EscapeCSV(kvp.Value)}\"");
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private string[] SplitCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentValue = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            result.Add(currentValue.ToString());
            return result.ToArray();
        }

        private string EscapeCSV(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\"", "\"\"").Replace("\n", "\\n");
        }

        #endregion
    }
}
#endif
