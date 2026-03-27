#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace DarkHome
{
    /// <summary>
    /// DIALOGUE CSV EDITOR - Import/Export Dialogues
    /// 
    /// MENU: Tools > DarkHome > CSV Tools > Dialogue Manager
    /// 
    /// ĐẶC BIỆT: Dialogue có cấu trúc phức tạp (nested Choices, Flags...)
    /// nên chúng ta sẽ export theo format riêng.
    /// </summary>
    public class DialogueCSVEditor : EditorWindow
    {
        private string _soFolder = "Assets/DarkHome/SO/Resources/Chapter1/Dialogues";
        private string _csvOutputFolder = "Assets/DarkHome/Editor/CSVData/Dialogues";
        private Vector2 _scrollPos;

        [MenuItem("Tools/DarkHome/CSV Tools/Dialogue Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<DialogueCSVEditor>("Dialogue CSV Manager");
            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("💬 DIALOGUE CSV MANAGER", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // ===== EXPORT SECTION =====
            DrawSection("📤 EXPORT Dialogues → CSV", () =>
            {
                EditorGUILayout.HelpBox(
                    "Export DialogueDataSO ra CSV.\n" +
                    "⚠️ Dialogue có structure phức tạp nên sẽ tạo 2 files:\n" +
                    "  1. DialogueNodes.csv (Nội dung thoại)\n" +
                    "  2. DialogueChoices.csv (Lựa chọn)",
                    MessageType.Info
                );

                _soFolder = EditorGUILayout.TextField("SO Folder:", _soFolder);
                _csvOutputFolder = EditorGUILayout.TextField("Output Folder:", _csvOutputFolder);

                if (GUILayout.Button("🔽 Export to CSV", GUILayout.Height(30)))
                {
                    ExportDialoguesToCSV();
                }
            });

            GUILayout.Space(20);

            // ===== LOCALIZATION EXTRACT =====
            DrawSection("🌐 EXTRACT Dialogue Texts (For Translation)", () =>
            {
                EditorGUILayout.HelpBox(
                    "Tạo file CSV chỉ chứa DialogueText để gửi cho Translator.\n" +
                    "Format: NodeID, DialogueText_EN, DialogueText_VN",
                    MessageType.Info
                );

                if (GUILayout.Button("📝 Extract Dialogue Texts", GUILayout.Height(30)))
                {
                    ExtractDialogueTexts();
                }
            });

            EditorGUILayout.EndScrollView();
        }

        // ==================== EXPORT ====================
        private void ExportDialoguesToCSV()
        {
            // Find all DialogueDataSO
            string[] guids = AssetDatabase.FindAssets("t:DialogueDataSO", new[] { _soFolder });
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Không tìm thấy DialogueDataSO nào!", "OK");
                return;
            }

            // Ensure output folder exists
            if (!Directory.Exists(_csvOutputFolder))
            {
                Directory.CreateDirectory(_csvOutputFolder);
            }

            var nodesData = new StringBuilder();
            var choicesData = new StringBuilder();

            // Headers
            nodesData.AppendLine("SourceFile,NpcId,NodeId,IdSpeaker,DialogueText,NextId,IsStartNode,Priority,IsRepeatable");
            choicesData.AppendLine("SourceFile,ParentNodeId,ChoiceText,NextNodeID,IsHidden");

            int nodeCount = 0;
            int choiceCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var dialogueSO = AssetDatabase.LoadAssetAtPath<DialogueDataSO>(path);
                if (dialogueSO == null) continue;

                string fileName = Path.GetFileName(path);

                foreach (var node in dialogueSO.Nodes)
                {
                    // Export node
                    nodesData.AppendLine(
                        $"\"{fileName}\"," +
                        $"\"{dialogueSO.NpcId}\"," +
                        $"\"{node.NodeId}\"," +
                        $"\"{node.IdSpeaker}\"," +
                        $"\"{EscapeCSV(node.DialogueText)}\"," +
                        $"\"{node.NextId}\"," +
                        $"{node.IsStartNode}," +
                        $"{node.Priority}," +
                        $"{node.IsRepeatable}"
                    );
                    nodeCount++;

                    // Export choices
                    if (node.Choices != null)
                    {
                        foreach (var choice in node.Choices)
                        {
                            choicesData.AppendLine(
                                $"\"{fileName}\"," +
                                $"\"{node.NodeId}\"," +
                                $"\"{EscapeCSV(choice.ChoiceText)}\"," +
                                $"\"{choice.NextNodeID}\"," +
                                $"{choice.IsHidden}"
                            );
                            choiceCount++;
                        }
                    }
                }
            }

            // Write files
            File.WriteAllText(Path.Combine(_csvOutputFolder, "DialogueNodes.csv"), nodesData.ToString());
            File.WriteAllText(Path.Combine(_csvOutputFolder, "DialogueChoices.csv"), choicesData.ToString());
            
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", 
                $"Exported:\n" +
                $"  • {nodeCount} dialogue nodes\n" +
                $"  • {choiceCount} choices\n\n" +
                $"to folder: {_csvOutputFolder}",
                "OK");
        }

        // ==================== LOCALIZATION ====================
        private void ExtractDialogueTexts()
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueDataSO", new[] { _soFolder });
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Không tìm thấy DialogueDataSO!", "OK");
                return;
            }

            var locData = new StringBuilder();
            locData.AppendLine("NodeID,Speaker,DialogueText_EN,DialogueText_VN,ChoiceText_EN,ChoiceText_VN");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var dialogueSO = AssetDatabase.LoadAssetAtPath<DialogueDataSO>(path);
                if (dialogueSO == null) continue;

                foreach (var node in dialogueSO.Nodes)
                {
                    // Node dialogue text
                    locData.AppendLine(
                        $"\"{node.NodeId}\"," +
                        $"\"{node.IdSpeaker}\"," +
                        $"\"{EscapeCSV(node.DialogueText)}\"," +
                        $"\"\"," + // VN text (empty for now)
                        $"\"\"," + // Choice EN (leave empty if no choices)
                        $"\"\"" // Choice VN
                    );

                    // Choices
                    if (node.Choices != null && node.Choices.Count > 0)
                    {
                        foreach (var choice in node.Choices)
                        {
                            locData.AppendLine(
                                $"\"{node.NodeId}_choice\"," +
                                $"\"CHOICE\"," +
                                $"\"\"," +
                                $"\"\"," +
                                $"\"{EscapeCSV(choice.ChoiceText)}\"," +
                                $"\"\"" // VN choice text
                            );
                        }
                    }
                }
            }

            // Save
            string locPath = "Assets/StreamingAssets/Localization/Dialogues_Localization_Template.csv";
            string dir = Path.GetDirectoryName(locPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(locPath, locData.ToString());
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", 
                "Localization template created!\n\n" +
                "Gửi file này cho Translator để họ điền cột _VN.",
                "OK");
        }

        // ==================== HELPERS ====================
        private string EscapeCSV(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Replace("\"", "\"\"").Replace("\n", "\\n");
        }

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
