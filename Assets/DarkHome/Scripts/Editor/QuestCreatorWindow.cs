// Deleted
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DarkHome
{
    /// <summary>
    /// QUEST CREATOR - Unity Editor Window for creating quests with localization support
    /// 
    /// WORKFLOW:
    /// 1. User enters quest info + English text
    /// 2. Tool auto-generates localization keys (QUEST_ID_name, QUEST_ID_desc, etc)
    /// 3. Saves Quest to QuestDataSO with KEYS (not text)
    /// 4. Appends English text to EN.csv
    /// 
    /// MENU: Tools > DarkHome > Quest Creator
    /// </summary>
    public class QuestCreatorWindow : EditorWindow
    {
        #region Serialized Fields

        private QuestDataSO _targetQuestDataSO;
        private Vector2 _scrollPos;

        // Current Quest being edited
        private Quest _currentQuest = new Quest();

        // Temporary text fields (not saved in SO, only used for generating CSV entries)
        private string _tempQuestName = "";
        private string _tempQuestDesc = "";
        private List<string> _tempObjectiveDescs = new List<string>();

        #endregion

        #region Menu Item

        [MenuItem("Tools/DarkHome/Quest Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuestCreatorWindow>("Quest Creator");
            window.minSize = new Vector2(600, 700);
        }

        #endregion

        #region GUI Drawing

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("ðŸŽ¯ QUEST CREATOR", EditorStyles.largeLabel);
            GUILayout.Space(10);

            // ===== Target QuestDataSO selector =====
            DrawTargetSOSelector();

            if (_targetQuestDataSO == null)
            {
                EditorGUILayout.HelpBox(
                    "Please select a QuestDataSO to add quests to.\n\n" +
                    "Create one: Assets â†’ Create â†’ SO â†’ Quest â†’ Quest Data",
                    MessageType.Warning
                );
                EditorGUILayout.EndScrollView();
                return;
            }

            GUILayout.Space(10);

            // ===== Quest Basic Info =====
            DrawQuestBasicInfo();

            GUILayout.Space(10);

            // ===== Objectives Section =====
            DrawObjectivesSection();

            GUILayout.Space(10);

            // ===== Flags Section =====
            DrawFlagsSection();

            GUILayout.Space(20);

            // ===== Save Button =====
            DrawSaveButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTargetSOSelector()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ðŸ“ Target QuestDataSO", EditorStyles.boldLabel);

            _targetQuestDataSO = EditorGUILayout.ObjectField(
                "Quest Data SO",
                _targetQuestDataSO,
                typeof(QuestDataSO),
                false
            ) as QuestDataSO;

            if (_targetQuestDataSO != null)
            {
                EditorGUILayout.LabelField("Chapter ID:", _targetQuestDataSO.chapterId);
                EditorGUILayout.LabelField("Current Quests:", _targetQuestDataSO.quests.Count.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuestBasicInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ðŸ“ Quest Basic Info", EditorStyles.boldLabel);

            // Quest ID (used for key generation)
            EditorGUILayout.LabelField("Quest ID (for keys)", EditorStyles.miniBoldLabel);
            _currentQuest.Id = EditorGUILayout.TextField("ID:", _currentQuest.Id);
            EditorGUILayout.HelpBox(
                "Example: QUEST_C1_FIND_KEY\n" +
                "Will generate keys: QUEST_C1_FIND_KEY_name, QUEST_C1_FIND_KEY_desc",
                MessageType.Info
            );

            GUILayout.Space(5);

            // Quest Name (English - temp)
            EditorGUILayout.LabelField("Quest Name (English)", EditorStyles.miniBoldLabel);
            _tempQuestName = EditorGUILayout.TextField(_tempQuestName);

            // Quest Description (English - temp)
            EditorGUILayout.LabelField("Description (English)", EditorStyles.miniBoldLabel);
            _tempQuestDesc = EditorGUILayout.TextArea(_tempQuestDesc, GUILayout.Height(60));

            GUILayout.Space(5);

            // Quest Type
            _currentQuest.Type = (EQuestType)EditorGUILayout.EnumPopup("Type:", _currentQuest.Type);

            // Display Mode
            _currentQuest.DisplayMode = (EQuestDisplayMode)EditorGUILayout.EnumPopup(
                "Display Mode:",
                _currentQuest.DisplayMode
            );

            // Is Hidden
            _currentQuest.IsHidden = EditorGUILayout.Toggle("Is Hidden:", _currentQuest.IsHidden);

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectivesSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ðŸŽ¯ Objectives", EditorStyles.boldLabel);

            if (_currentQuest.Objectives == null)
                _currentQuest.Objectives = new List<QuestObjective>();

            if (_tempObjectiveDescs == null)
                _tempObjectiveDescs = new List<string>();

            // Ensure temp list matches objectives count
            while (_tempObjectiveDescs.Count < _currentQuest.Objectives.Count)
                _tempObjectiveDescs.Add("");

            for (int i = 0; i < _currentQuest.Objectives.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Label($"Objective {i + 1}", EditorStyles.miniBoldLabel);

                var obj = _currentQuest.Objectives[i];

                // Description (EN - temp)
                EditorGUILayout.LabelField("Description (English):");
                _tempObjectiveDescs[i] = EditorGUILayout.TextField(_tempObjectiveDescs[i]);

                // Type
                obj.Type = (EQuestObjectiveType)EditorGUILayout.EnumPopup("Type:", obj.Type);

                // Target ID (for collect/interact types)
                obj.TargetID = EditorGUILayout.TextField("Target ID:", obj.TargetID);

                // Required Amount
                obj.RequiredAmount = EditorGUILayout.IntField("Required Amount:", obj.RequiredAmount);

                // Remove button
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove Objective"))
                {
                    _currentQuest.Objectives.RemoveAt(i);
                    _tempObjectiveDescs.RemoveAt(i);
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }

            // Add Objective button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+ Add Objective", GUILayout.Height(30)))
            {
                _currentQuest.Objectives.Add(new QuestObjective { RequiredAmount = 1 });
                _tempObjectiveDescs.Add("");
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawFlagsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("ðŸš© Quest Flags", EditorStyles.boldLabel);

            // Required Flags
            EditorGUILayout.LabelField("Required Flags (to unlock):", EditorStyles.miniBoldLabel);
            DrawFlagList(ref _currentQuest.RequiredFlags);

            GUILayout.Space(5);

            // On Complete Triggers
            EditorGUILayout.LabelField("On Complete Triggers (events):", EditorStyles.miniBoldLabel);
            DrawFlagList(ref _currentQuest.OnCompleteTriggers);

            EditorGUILayout.EndVertical();
        }

        private void DrawFlagList(ref List<FlagData> flagList)
        {
            if (flagList == null)
                flagList = new List<FlagData>();

            for (int i = 0; i < flagList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var flag = flagList[i];
                flag.FlagID = EditorGUILayout.TextField(flag.FlagID);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    flagList.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Flag", GUILayout.Height(25)))
            {
                flagList.Add(new FlagData("NEW_FLAG", EFlagScope.Global));
            }
        }

        private void DrawSaveButton()
        {
            GUI.backgroundColor = Color.cyan;

            if (GUILayout.Button("ðŸ’¾ Save Quest + Export to CSV", GUILayout.Height(50)))
            {
                SaveQuestWithLocalization();
            }

            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            if (GUILayout.Button("Reset Form", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Form",
                    "Clear all fields and start fresh?",
                    "Yes", "Cancel"))
                {
                    ResetForm();
                }
            }
        }

        #endregion

        #region Save Logic

        private void SaveQuestWithLocalization()
        {
            // Validation
            if (string.IsNullOrEmpty(_currentQuest.Id))
            {
                EditorUtility.DisplayDialog("Error", "Quest ID is required!", "OK");
                return;
            }

            if (string.IsNullOrEmpty(_tempQuestName))
            {
                EditorUtility.DisplayDialog("Error", "Quest Name (English) is required!", "OK");
                return;
            }

            // Check duplicate ID
            if (_targetQuestDataSO.quests.Any(q => q.Id == _currentQuest.Id))
            {
                EditorUtility.DisplayDialog("Error", $"Quest ID '{_currentQuest.Id}' already exists!", "OK");
                return;
            }

            // 1. Generate localization keys
            _currentQuest.QuestNameKey = $"{_currentQuest.Id}_name";
            _currentQuest.DescriptionKey = $"{_currentQuest.Id}_desc";

            for (int i = 0; i < _currentQuest.Objectives.Count; i++)
            {
                _currentQuest.Objectives[i].DescriptionKey = $"{_currentQuest.Id}_OBJ{i + 1}";
            }

            // 2. Append to EN.csv
            string csvPath = Path.Combine(Application.dataPath, "StreamingAssets/Localization/EN.csv");

            try
            {
                AppendToCSV(csvPath, _currentQuest.QuestNameKey, _tempQuestName);
                AppendToCSV(csvPath, _currentQuest.DescriptionKey, _tempQuestDesc);

                for (int i = 0; i < _tempObjectiveDescs.Count; i++)
                {
                    if (i < _currentQuest.Objectives.Count)
                    {
                        AppendToCSV(csvPath, _currentQuest.Objectives[i].DescriptionKey, _tempObjectiveDescs[i]);
                    }
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("CSV Error", $"Failed to append to CSV:\n{e.Message}", "OK");
                return;
            }

            // 3. Set initial status
            _currentQuest.Status = EQuestStatus.Inactive;

            // 4. Deep copy quest (avoid reference issues)
            string json = JsonUtility.ToJson(_currentQuest);
            Quest questCopy = JsonUtility.FromJson<Quest>(json);

            // 5. Add to SO
            _targetQuestDataSO.quests.Add(questCopy);

            // 6. Mark dirty and save
            EditorUtility.SetDirty(_targetQuestDataSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Success!
            EditorUtility.DisplayDialog(
                "Success!",
                $"Quest '{_tempQuestName}' created!\n\n" +
                $"Keys generated:\n" +
                $"  â€¢ {_currentQuest.QuestNameKey}\n" +
                $"  â€¢ {_currentQuest.DescriptionKey}\n" +
                $"  â€¢ {_currentQuest.Objectives.Count} objective keys\n\n" +
                $"English text added to EN.csv\n\n" +
                $"Next step: Export translations for VN.csv",
                "OK"
            );

            // Reset form
            ResetForm();
        }

        private void AppendToCSV(string csvPath, string key, string text)
        {
            // Ensure CSV exists
            if (!File.Exists(csvPath))
            {
                string dir = Path.GetDirectoryName(csvPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(csvPath, "Key,Text\n");
            }

            // Check if key already exists
            string[] lines = File.ReadAllLines(csvPath);
            bool keyExists = lines.Any(line => line.StartsWith(key + ","));

            if (keyExists)
            {
                Debug.LogWarning($"Key '{key}' already exists in CSV, skipping...");
                return;
            }

            // Escape text for CSV (handle quotes and newlines)
            string escapedText = text.Replace("\"", "\"\"").Replace("\n", "\\n");

            // Append new entry
            File.AppendAllText(csvPath, $"{key},\"{escapedText}\"\n");

            Debug.Log($"âœ… Added to EN.csv: {key}");
        }

        private void ResetForm()
        {
            _currentQuest = new Quest();
            _tempQuestName = "";
            _tempQuestDesc = "";
            _tempObjectiveDescs = new List<string>();
        }

        #endregion
    }
}
#endif

