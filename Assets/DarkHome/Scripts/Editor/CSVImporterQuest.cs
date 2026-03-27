using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;  // 🔥 NEW: For GroupBy support

namespace DarkHome.Editor
{
    public class CSVImporterQuest : EditorWindow
    {
        private const string CSV_PATH = "Assets/DarkHome/Data/CSV/Quests.csv";
        private const string OBJECTIVES_CSV_PATH = "Assets/DarkHome/Data/CSV/QuestObjectives.csv";
        private const string OUTPUT_FOLDER = "Assets/DarkHome/SO/Resources/Chapter1";

        private string _csvPath = CSV_PATH;
        private string _outputFolder = OUTPUT_FOLDER;
        private bool _overwriteExisting = false;

        [MenuItem("Tools/DarkHome/Import Quests CSV")]
        public static void ShowWindow()
        {
            GetWindow<CSVImporterQuest>("CSV Quest Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV Quest Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _csvPath = EditorGUILayout.TextField("CSV Path:", _csvPath);
            _outputFolder = EditorGUILayout.TextField("Output Folder:", _outputFolder);
            _overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing:", _overwriteExisting);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Imports Quests.csv + QuestObjectives.csv → QuestDataSO\n" +
                "Output: {OutputFolder}/{ChapterID}_Quests.asset",
                MessageType.Info);
            EditorGUILayout.Space();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("📜 Import Quests from CSV", GUILayout.Height(40)))
            {
                ImportQuests();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ImportQuests()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", $"CSV not found: {_csvPath}", "OK");
                return;
            }

            if (!File.Exists(OBJECTIVES_CSV_PATH))
            {
                EditorUtility.DisplayDialog("Error", $"Objectives CSV not found: {OBJECTIVES_CSV_PATH}", "OK");
                return;
            }

            // Parse Quests.csv
            List<QuestCSVRow> questRows = ParseQuestsCSV(_csvPath);
            if (questRows.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No quests in CSV", "OK");
                return;
            }

            // Parse QuestObjectives.csv
            List<ObjectiveCSVRow> objectiveRows = ParseObjectivesCSV(OBJECTIVES_CSV_PATH);
            Debug.Log($"� Loaded {objectiveRows.Count} objectives from CSV");

            EnsureFolderExists(_outputFolder);

            int created = 0, updated = 0, skipped = 0;

            // Create Quest objects
            List<Quest> allQuests = new List<Quest>();
            string chapterId = "";

            foreach (var questRow in questRows)
            {
                // Store chapterId from first quest (all should be same chapter)
                if (string.IsNullOrEmpty(chapterId))
                {
                    chapterId = questRow.ChapterID;
                }

                // Create Quest from row
                Quest quest = new Quest
                {
                    Id = questRow.QuestID,
                    QuestNameKey = questRow.NameKey,
                    DescriptionKey = questRow.DescKey,
                    Type = ParseQuestType(questRow.Type),
                    Status = EQuestStatus.Inactive,
                    IsHidden = questRow.IsHidden.ToUpper() == "TRUE",
                    DisplayMode = ParseDisplayMode(questRow.DisplayMode),
                    RequiredFlags = ParseFlags(questRow.RequiredFlags),
                    OnCompleteTriggers = ParseFlags(questRow.OnCompleteTriggers),
                    Objectives = new List<QuestObjective>()
                };

                // Find and add objectives for this quest
                var questObjectives = objectiveRows.Where(obj => obj.QuestID == quest.Id).ToList();
                foreach (var objRow in questObjectives)
                {
                    QuestObjective objective = CreateObjectiveFromRow(objRow);
                    quest.Objectives.Add(objective);
                }

                allQuests.Add(quest);
                Debug.Log($"✅ Parsed Quest: {quest.Id} with {quest.Objectives.Count} objectives");
            }

            // 🔥 NEW: Create/Update SINGLE QuestDataSO for entire chapter
            string fileName = $"{chapterId}_Quests.asset";
            string fullPath = Path.Combine(_outputFolder, fileName);

            QuestDataSO questData = AssetDatabase.LoadAssetAtPath<QuestDataSO>(fullPath);

            if (questData == null)
            {
                questData = ScriptableObject.CreateInstance<QuestDataSO>();
                questData.chapterId = chapterId;
                questData.quests = allQuests;
                AssetDatabase.CreateAsset(questData, fullPath);
                created++;
                Debug.Log($"🎉 Created QuestDataSO: {fileName} with {allQuests.Count} quests");
            }
            else
            {
                if (!_overwriteExisting)
                {
                    // Skip update - respect global toggle
                    skipped++;
                    Debug.LogWarning($"Skipped {fileName} (already exists, overwrite disabled)");

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Info",
                        $"{fileName} already exists.\nEnable 'Overwrite Existing' to update.",
                        "OK");
                    return;
                }

                questData.chapterId = chapterId;
                questData.quests = allQuests;
                EditorUtility.SetDirty(questData);
                updated++;
                Debug.Log($"🔄 Updated QuestDataSO: {fileName} with {allQuests.Count} quests");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Import Complete!\n\n" +
                $"Created: {created}\n" +
                $"Updated: {updated}\n" +
                $"Skipped: {skipped}\n\n" +
                $"File: {fileName}\n" +
                $"Total Quests: {allQuests.Count}\n" +
                $"Total Objectives: {allQuests.Sum(q => q.Objectives.Count)}\n\n" +
                $"Output: {_outputFolder}";

            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        // 🔥 NEW: Helper method to create objective from CSV row
        private QuestObjective CreateObjective(QuestCSVRow row)
        {
            return new QuestObjective
            {
                Type = ParseObjectiveType(row.ObjectiveType),
                DescriptionKey = row.ObjectiveDescKey,
                TargetID = row.TargetID,
                RequiredAmount = string.IsNullOrEmpty(row.RequiredAmount) ? 1 : int.Parse(row.RequiredAmount),
                CurrentAmount = 0,
                CompletionFlags = ParseFlags(row.CompletionFlags),
                OnCompleteTriggers = ParseFlags(row.OnCompleteTriggers_Obj),
                HideOnCompletion = row.HideOnCompletion.ToUpper() == "TRUE"
            };
        }

        // 🔥 NEW: Parse objective type from string
        private EQuestObjectiveType ParseObjectiveType(string typeStr)
        {
            switch (typeStr)
            {
                case "Collect": return EQuestObjectiveType.Collect;
                case "Talk": return EQuestObjectiveType.Talk;
                case "MoveTo": return EQuestObjectiveType.MoveTo;
                case "Kill": return EQuestObjectiveType.Kill;
                case "Interact": return EQuestObjectiveType.Interact;
                default:
                    Debug.LogWarning($"Unknown ObjectiveType: {typeStr}, defaulting to Collect");
                    return EQuestObjectiveType.Collect;
            }
        }


        // 🔥 NEW: Parse Quests.csv
        private List<QuestCSVRow> ParseQuestsCSV(string path)
        {
            List<QuestCSVRow> rows = new List<QuestCSVRow>();
            string[] lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 9) continue;  // 🔥 At least QuestID + Quest Info

                // CSV Format: QuestID,ChapterID,QuestNameKey,DescriptionKey,Type,Status,IsHidden,DisplayMode,RequiredFlags,OnCompleteTriggers,Objectives
                rows.Add(new QuestCSVRow
                {
                    // Quest Info
                    QuestID = cols[0].Trim(),
                    ChapterID = cols[1].Trim(),
                    NameKey = cols[2].Trim(),
                    DescKey = cols[3].Trim(),
                    Type = cols[4].Trim(),
                    IsHidden = cols[6].Trim(),
                    DisplayMode = cols[7].Trim(),
                    RequiredFlags = cols.Length > 8 ? cols[8].Trim() : "",
                    OnCompleteTriggers = cols.Length > 9 ? cols[9].Trim() : "",

                    // 🔥 NEW: Objective Info
                    ObjectiveType = cols.Length > 9 ? cols[9].Trim() : "",
                    ObjectiveDescKey = cols.Length > 10 ? cols[10].Trim() : "",
                    TargetID = cols.Length > 11 ? cols[11].Trim() : "",
                    RequiredAmount = cols.Length > 12 ? cols[12].Trim() : "1",
                    CompletionFlags = cols.Length > 13 ? cols[13].Trim() : "",
                    OnCompleteTriggers_Obj = cols.Length > 14 ? cols[14].Trim() : "",
                    HideOnCompletion = cols.Length > 15 ? cols[15].Trim() : "FALSE"
                });
            }

            return rows;
        }

        // 🔥 NEW: Parse QuestObjectives.csv
        private List<ObjectiveCSVRow> ParseObjectivesCSV(string path)
        {
            List<ObjectiveCSVRow> rows = new List<ObjectiveCSVRow>();
            string[] lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');
                if (cols.Length < 8) continue;  // QuestID, ObjectiveID, Type, TargetID, RequiredCount, OnCompleteTriggers, Description, IsOptional

                // CSV Format: ObjectiveID,QuestID,Type,DescriptionKey,TargetID,RequiredAmount,CompletionFlags,RequiredFlagsToAppear
                rows.Add(new ObjectiveCSVRow
                {
                    ObjectiveID = cols[0].Trim(),
                    QuestID = cols[1].Trim(),
                    Type = cols[2].Trim(),
                    DescriptionKey = cols[3].Trim(),
                    TargetID = cols[4].Trim(),
                    RequiredCount = cols[5].Trim(),
                    CompletionFlags = cols[6].Trim(),
                    RequiredFlagsToAppear = cols.Length > 7 ? cols[7].Trim() : "",
                    OnCompleteTriggers = cols.Length > 8 ? cols[8].Trim() : ""
                });
            }

            return rows;
        }

        // Helper: Create objective from ObjectiveCSVRow
        private QuestObjective CreateObjectiveFromRow(ObjectiveCSVRow row)
        {
            return new QuestObjective
            {
                Type = ParseObjectiveType(row.Type),
                DescriptionKey = row.DescriptionKey,
                TargetID = row.TargetID,
                RequiredAmount = string.IsNullOrEmpty(row.RequiredCount) ? 1 : int.Parse(row.RequiredCount),
                CurrentAmount = 0,
                CompletionFlags = ParseFlags(row.CompletionFlags),  // ✅ check flags
                OnCompleteTriggers = ParseFlags(row.OnCompleteTriggers),
                RequiredFlagsToAppear = ParseFlags(row.RequiredFlagsToAppear),
                HideOnCompletion = false
            };
        }

        private EQuestType ParseQuestType(string typeStr)
        {
            return typeStr.ToUpper() == "MAIN" ? EQuestType.Main : EQuestType.Side;
        }

        private EQuestDisplayMode ParseDisplayMode(string modeStr)
        {
            switch (modeStr)
            {
                case "Sequential": return EQuestDisplayMode.Sequential;
                case "Parallel": return EQuestDisplayMode.Parallel;
                case "Custom": return EQuestDisplayMode.Custom;
                default: return EQuestDisplayMode.Sequential;
            }
        }

        private List<FlagData> ParseFlags(string flagsStr)
        {
            List<FlagData> flags = new List<FlagData>();
            if (string.IsNullOrEmpty(flagsStr)) return flags;

            string[] flagIDs = flagsStr.Split(';');
            foreach (string flagID in flagIDs)
            {
                string trimmed = flagID.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                flags.Add(new FlagData(trimmed, trimmed.Contains("GLOBAL") ? EFlagScope.Global : EFlagScope.Local));
            }

            return flags;
        }

        private void EnsureFolderExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        [System.Serializable]
        private class QuestCSVRow
        {
            // Quest Info (filled in first row only)
            public string QuestID;
            public string NameKey;
            public string DescKey;
            public string ChapterID;
            public string Type;
            public string IsHidden;
            public string DisplayMode;
            public string RequiredFlags;
            public string OnCompleteTriggers;

            // 🔥 NEW: Objective Info (filled in all rows)
            public string ObjectiveType;
            public string ObjectiveDescKey;
            public string TargetID;
            public string RequiredAmount;
            public string CompletionFlags;
            public string OnCompleteTriggers_Obj;
            public string HideOnCompletion;
        }

        [System.Serializable]
        private class ObjectiveCSVRow
        {
            public string QuestID;
            public string ObjectiveID;
            public string Type;
            public string DescriptionKey;
            public string TargetID;
            public string RequiredCount;
            public string CompletionFlags;        // col 6: flags cần CÓ để complete
            public string RequiredFlagsToAppear;  // col 7: flags cần CÓ để hiện thị
            public string OnCompleteTriggers;     // col 8: flags grant khi complete
        }
    }
}
