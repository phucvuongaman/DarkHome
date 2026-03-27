#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DarkHome.Editor
{
    public class CSVImporterDialogue : EditorWindow
    {
        private const string CSV_PATH = "Assets/DarkHome/Data/CSV/Dialogues.csv";
        private const string CHOICES_CSV_PATH = "Assets/DarkHome/Data/CSV/DialogueChoices.csv";
        private const string OUTPUT_FOLDER = "Assets/DarkHome/SO/Resources/Chapter1/Dialogues";

        private string _csvPath = CSV_PATH;
        private string _choicesCsvPath = CHOICES_CSV_PATH;
        private string _outputFolder = OUTPUT_FOLDER;
        private bool _overwriteExisting = false;

        [MenuItem("Tools/DarkHome/Import Dialogues CSV")]
        public static void ShowWindow()
        {
            GetWindow<CSVImporterDialogue>("CSV Dialogue Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("💬 CSV Dialogue Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _csvPath = EditorGUILayout.TextField("Dialogues CSV:", _csvPath);
            _choicesCsvPath = EditorGUILayout.TextField("Choices CSV:", _choicesCsvPath);
            _outputFolder = EditorGUILayout.TextField("Output Folder:", _outputFolder);
            _overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing:", _overwriteExisting);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Imports Dialogues.csv + DialogueChoices.csv → DialogueDataSO per NPC\n" +
                "Output: {OutputFolder}/{DialogueID}.asset",
                MessageType.Info);
            EditorGUILayout.Space();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("💬 Import Dialogues from CSV", GUILayout.Height(40)))
            {
                ImportDialogues();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ImportDialogues()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", $"CSV not found: {_csvPath}", "OK");
                return;
            }

            List<DialogueCSVRow> rows = ParseCSV(_csvPath);
            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No dialogues in CSV", "OK");
                return;
            }

            // Parse choices if file exists
            Dictionary<string, List<ChoiceCSVRow>> choicesByNode = new Dictionary<string, List<ChoiceCSVRow>>();
            if (File.Exists(_choicesCsvPath))
            {
                var choiceRows = ParseChoicesCSV(_choicesCsvPath);
                choicesByNode = choiceRows.GroupBy(c => c.ParentNodeID)
                    .ToDictionary(g => g.Key, g => g.ToList());
                Debug.Log($"Loaded {choiceRows.Count} choices from DialogueChoices.csv");
            }

            EnsureFolderExists(_outputFolder);

            int created = 0, updated = 0, skipped = 0;
            int totalDialogues = 0, totalChoices = 0;

            // Group by DialogueID (each DialogueID = 1 DialogueDataSO file)
            var dialogueGroups = rows.GroupBy(r => r.DialogueID);

            foreach (var group in dialogueGroups)
            {
                string dialogueId = group.Key;
                string speakerId = group.First().SpeakerID; // Changed from NpcID

                string fileName = $"{dialogueId}.asset";
                string fullPath = Path.Combine(_outputFolder, fileName);

                DialogueDataSO dialogueSO;

                if (File.Exists(fullPath))
                {
                    if (!_overwriteExisting)
                    {
                        // Skip existing file
                        skipped++;
                        Debug.LogWarning($"Skipped {dialogueId} (already exists, overwrite disabled)");
                        continue; // Skip to next dialogue group
                    }
                    else
                    {
                        // Update existing
                        dialogueSO = AssetDatabase.LoadAssetAtPath<DialogueDataSO>(fullPath);
                        updated++;
                    }
                }
                else
                {
                    // Create new
                    dialogueSO = ScriptableObject.CreateInstance<DialogueDataSO>();
                    AssetDatabase.CreateAsset(dialogueSO, fullPath);
                    created++;
                }

                dialogueSO.NpcId = speakerId;
                dialogueSO.Nodes = new List<DialogueNode>();

                // Create DialogueNode from each row
                foreach (var row in group)
                {
                    var node = new DialogueNode
                    {
                        NodeId = row.NodeID,
                        IdSpeaker = speakerId,
                        DialogueTextKey = row.TextKey,
                        NextId = row.NextNodeID,
                        IsStartNode = row.IsStartNode,  // ✅ Từ CSV column 10, không tự suy
                        Priority = row.Priority,
                        IsRepeatable = row.IsRepeatable,
                        RequiredFlags = ParseFlags(row.RequiredFlags),
                        GrantedFlags = ParseFlags(row.GrantedFlags),
                        Choices = new List<Choice>()
                    };

                    // Attach choices if available
                    if (choicesByNode.ContainsKey(row.NodeID))
                    {
                        foreach (var choiceRow in choicesByNode[row.NodeID])
                        {
                            var choice = new Choice
                            {
                                ChoiceId = choiceRow.ChoiceID,
                                ChoiceTextKey = choiceRow.ChoiceTextKey,
                                NextNodeID = choiceRow.NextNodeID,
                                RequiredFlags = ParseFlags(choiceRow.RequiredFlags),
                                HidingFlags = ParseFlags(choiceRow.HidingFlags),
                                OnSelectTriggers = ParseFlags(choiceRow.OnSelectTriggers),
                                IsHidden = choiceRow.IsHidden
                            };
                            node.Choices.Add(choice);
                        }
                    }

                    dialogueSO.Nodes.Add(node);
                    totalDialogues++;
                    totalChoices += node.Choices.Count;
                }

                EditorUtility.SetDirty(dialogueSO);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Import Complete!\n\n" +
                $"Created: {created} NPCs\n" +
                $"Updated: {updated} NPCs\n" +
                $"Skipped: {skipped} NPCs\n\n" +
                $"Total Dialogues: {totalDialogues}\n" +
                $"Total Choices: {totalChoices}\n\n" +
                $"Output: {_outputFolder}";

            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        // Parse Dialogues CSV - UPDATED for NEW format
        private List<DialogueCSVRow> ParseCSV(string path)
        {
            var rows = new List<DialogueCSVRow>();
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 11) continue; // 11 columns: 0-DialogueID, 1-SpeakerID, 2-NodeID, 3-TextKey, 4-NextNodeID, 5-RequiredFlags, 6-GrantedFlags, 7-Priority, 8-IsRepeatable, 9-TriggerEvents, 10-IsStartNode

                var row = new DialogueCSVRow
                {
                    DialogueID = parts[0].Trim(),
                    SpeakerID = parts[1].Trim(),
                    NodeID = parts[2].Trim(),
                    TextKey = parts[3].Trim(),
                    NextNodeID = parts[4].Trim(),
                    RequiredFlags = parts[5].Trim(),
                    GrantedFlags = parts[6].Trim(),
                    Priority = int.TryParse(parts[7].Trim(), out int p) ? p : 0,
                    IsRepeatable = parts[8].Trim().ToUpper() == "TRUE",
                    TriggerEvents = parts[9].Trim(),
                    IsStartNode = parts[10].Trim().ToUpper() == "TRUE"  // ✅ Đọc trực tiếp từ CSV, không tự suy
                };

                rows.Add(row);
            }

            return rows;
        }

        // Parse DialogueChoices CSV
        private List<ChoiceCSVRow> ParseChoicesCSV(string path)
        {
            var rows = new List<ChoiceCSVRow>();
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 10) continue;  // Now need 10 columns (added OnSelectTriggers)

                var row = new ChoiceCSVRow
                {
                    DialogueID = parts[0].Trim(),
                    ParentNodeID = parts[1].Trim(),
                    ChoiceID = parts[2].Trim(),
                    ChoiceTextKey = parts[3].Trim(),
                    NextNodeID = parts[4].Trim(),
                    RequiredFlags = parts[5].Trim(),
                    HidingFlags = parts[6].Trim(),
                    GrantedFlags = parts[7].Trim(),
                    OnSelectTriggers = parts[8].Trim(),  // NEW: OnSelectTriggers column
                    IsHidden = parts[9].Trim().ToUpper() == "TRUE"  // Shifted to column 9
                };

                rows.Add(row);
            }

            return rows;
        }

        // Parse flags (semicolon separated)
        private List<FlagData> ParseFlags(string flagsStr)
        {
            var flags = new List<FlagData>();
            if (string.IsNullOrWhiteSpace(flagsStr)) return flags;

            var flagIds = flagsStr.Split(';');
            foreach (var flagId in flagIds)
            {
                string trimmed = flagId.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    flags.Add(new FlagData(trimmed, EFlagScope.Local));
                }
            }

            return flags;
        }

        private void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // CSV Row classes - FINAL consolidated format
        private class DialogueCSVRow
        {
            public string DialogueID;    // col 0: DIALOGUE_MIKA (grouped into 1 SO file)
            public string SpeakerID;     // col 1: MIKA, PLAYER, TV...
            public string NodeID;        // col 2: DAY1_MORNING_1
            public string TextKey;       // col 3: DIALOGUE_MIKA_DAY1_MORNING_1 (localization key)
            public string NextNodeID;    // col 4: DAY1_MORNING_2 hoặc END
            public string RequiredFlags; // col 5: flags player cần có
            public string GrantedFlags;  // col 6: flags tự grant khi node hiển thị
            public int Priority;         // col 7: độ ưu tiên khi chọn StartNode
            public bool IsRepeatable;    // col 8: có lặp lại được không
            public string TriggerEvents; // col 9: event triggers (tương lai)
            public bool IsStartNode;     // col 10: ✅ cửa vào conversation
            // col 11 (Choices): bỏ qua — chỉ là human-readable reference, dùng DialogueChoices.csv
        }

        private class ChoiceCSVRow
        {
            public string DialogueID;
            public string ParentNodeID;
            public string ChoiceID;
            public string ChoiceTextKey;
            public string NextNodeID;
            public string RequiredFlags;
            public string HidingFlags;
            public string GrantedFlags;
            public string OnSelectTriggers;  // NEW: Event triggers on choice select
            public bool IsHidden;
        }
    }
}
#endif
