#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DarkHome.Editor
{
    /// <summary>
    /// CSV Importer Tool - Generates ObjectDataSO from Objects.csv
    /// Menu: Tools/DarkHome/Import Objects CSV
    /// </summary>
    public class CSVImporterObject : EditorWindow
    {
        private const string CSV_PATH = "Assets/DarkHome/Data/CSV/Objects.csv";
        private const string OUTPUT_FOLDER = "Assets/DarkHome/SO/Resources/Chapter1/Objects";

        private string _csvPath = CSV_PATH;
        private string _outputFolder = OUTPUT_FOLDER;
        private bool _overwriteExisting = false;

        [MenuItem("Tools/DarkHome/Import Objects CSV")]
        public static void ShowWindow()
        {
            GetWindow<CSVImporterObject>("CSV Object Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("🏠 CSV Object Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Settings
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _csvPath = EditorGUILayout.TextField("CSV Path:", _csvPath);
            _outputFolder = EditorGUILayout.TextField("Output Folder:", _outputFolder);
            _overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing:", _overwriteExisting);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Imports Objects.csv → ObjectDataSO per Object\n" +
                "Output: {OutputFolder}/{ObjectID}.asset\n\n" +
                "WORKFLOW:\n" +
                "1. Import CSV\n" +
                "2. Create GameObject (Cube)\n" +
                "3. Add BaseObject.cs\n" +
                "4. Drag ObjectDataSO to _objectData field\n" +
                "5. Done!",
                MessageType.Info);
            EditorGUILayout.Space();

            // Import button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🏠 Import Objects from CSV", GUILayout.Height(40)))
            {
                ImportObjects();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Output Folder"))
            {
                EditorUtility.RevealInFinder(_outputFolder);
            }
        }

        private void ImportObjects()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", $"CSV file not found: {_csvPath}", "OK");
                return;
            }

            // Ensure output folder exists
            if (!AssetDatabase.IsValidFolder(_outputFolder))
            {
                string[] folders = _outputFolder.Split('/');
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

            // Parse CSV
            List<ObjectCSVRow> rows = ParseCSV(_csvPath);

            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No objects found in CSV", "OK");
                return;
            }

            int created = 0;
            int updated = 0;
            int skipped = 0;

            // Generate SOs
            foreach (var row in rows)
            {
                string fileName = $"{row.ObjectID}.asset";
                string fullPath = Path.Combine(_outputFolder, fileName);

                // Check if exists
                ObjectDataSO existingData = AssetDatabase.LoadAssetAtPath<ObjectDataSO>(fullPath);

                if (existingData != null && !_overwriteExisting)
                {
                    skipped++;
                    continue;
                }

                ObjectDataSO objectData;
                if (existingData != null)
                {
                    objectData = existingData;
                    updated++;
                }
                else
                {
                    // ✅ Create correct SO type based on ObjectType
                    objectData = CreateSOByType(row.ObjectType);
                    AssetDatabase.CreateAsset(objectData, fullPath);
                    created++;
                }

                // Set common properties (all inherit from ObjectDataSO)
                objectData.objectID = row.ObjectID;
                objectData.localizationKey = row.ObjectID;
                objectData.requiredFlags = ParseFlags(row.RequiredFlags);
                objectData.hidingFlags = ParseFlags(row.HidingFlags);
                objectData.onInteractTriggers = ParseFlags(row.OnInteractTriggers);

                // ✅ Set type-specific fields
                if (objectData is ItemDataSO item)
                {
                    // Item specific
                    if (!string.IsNullOrEmpty(row.ItemType))
                        item.itemType = ParseItemType(row.ItemType);
                    if (!string.IsNullOrEmpty(row.ItemQuestKey))
                        item.questKey = row.ItemQuestKey;
                    // Note: prefab loaded from Resources path later
                    if (!string.IsNullOrEmpty(row.ItemEffects))
                        item.onPickupEffects = ParseItemEffects(row.ItemEffects);
                }
                else if (objectData is DoorDataSO door)
                {
                    // Door specific
                    if (!string.IsNullOrEmpty(row.NextSceneName))
                        door.nextSceneName = row.NextSceneName;
                    if (!string.IsNullOrEmpty(row.TargetSpawnID))
                        door.targetSpawnID = row.TargetSpawnID;
                    if (!string.IsNullOrEmpty(row.Description))
                        door.description = row.Description;
                }
                else if (objectData is BedDataSO bed)
                {
                    // Bed specific
                    if (!string.IsNullOrEmpty(row.LoopableChapters))
                        bed.loopableChapterIDs = new List<string>(row.LoopableChapters.Split(';'));
                    if (!string.IsNullOrEmpty(row.WakeUpSpawnID))
                        bed.wakeUpSpawnID = row.WakeUpSpawnID;
                    if (!string.IsNullOrEmpty(row.CantSleepDialogue))
                        bed.cantSleepDialogueID = row.CantSleepDialogue;
                }
                else if (objectData is InteractableDataSO interactable)
                {
                    // Interactable specific (SOFA, TV, FRIDGE, etc.)
                    if (!string.IsNullOrEmpty(row.Description))
                        interactable.examineDialogueKey = row.Description;
                }
                else if (objectData is AreaDataSO area)
                {
                    // TriggerArea specific
                    if (!string.IsNullOrEmpty(row.OnEnterTriggers))
                        area.onEnterTriggers = ParseFlags(row.OnEnterTriggers);
                    if (!string.IsNullOrEmpty(row.OnExitTriggers))
                        area.onExitTriggers = ParseFlags(row.OnExitTriggers);
                    if (!string.IsNullOrEmpty(row.TriggerOnce))
                        area.triggerOnce = bool.Parse(row.TriggerOnce);
                    else
                        area.triggerOnce = true; // Default to true if not specified
                }
                else if (objectData is PhysicalDoorDataSO physicalDoor)
                {
                    // PhysicalDoor has no specific fields beyond ObjectDataSO
                    // TargetSpawnID is set in PhysicalDoor.cs component, not SO
                    Debug.Log($"PhysicalDoorDataSO '{row.ObjectID}' - targetSpawnID set in component, not SO.");
                }

                EditorUtility.SetDirty(objectData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Import Complete!\n\n" +
                $"Created: {created}\n" +
                $"Updated: {updated}\n" +
                $"Skipped: {skipped}\n\n" +
                $"Total: {rows.Count}\n\n" +
                $"Output: {_outputFolder}";

            EditorUtility.DisplayDialog("Success", message, "OK");
            Debug.Log($"✅ Objects CSV Import: {created} created, {updated} updated, {skipped} skipped");
        }

        private List<ObjectCSVRow> ParseCSV(string path)
        {
            var rows = new List<ObjectCSVRow>();
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 17) continue; // 17 columns total

                var row = new ObjectCSVRow
                {
                    ObjectID = parts[0].Trim(),
                    ObjectType = parts[1].Trim(),
                    RequiredFlags = parts[2].Trim(),
                    HidingFlags = parts[3].Trim(),
                    OnInteractTriggers = parts[4].Trim(),
                    ItemType = parts[5].Trim(),
                    ItemQuestKey = parts[6].Trim(),
                    ItemPrefab = parts[7].Trim(),
                    ItemEffects = parts[8].Trim(),
                    NextSceneName = parts[9].Trim(),
                    TargetSpawnID = parts[10].Trim(),
                    LoopableChapters = parts[11].Trim(),
                    WakeUpSpawnID = parts[12].Trim(),
                    CantSleepDialogue = parts[13].Trim(),
                    OnEnterTriggers = parts[14].Trim(),
                    OnExitTriggers = parts[15].Trim(),
                    TriggerOnce = parts[16].Trim(),
                    DestroyOnPickup = parts.Length > 17 ? parts[17].Trim() : "",
                    Description = parts.Length > 18 ? parts[18].Trim() : ""
                };

                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Parse EItemType from string.
        /// </summary>
        private EItemType ParseItemType(string typeStr)
        {
            switch (typeStr.Trim())
            {
                case "Key": return EItemType.Key;
                case "PuzzleItem": return EItemType.PuzzleItem;
                case "Note": return EItemType.Note;
                case "QuestItem": return EItemType.QuestItem;
                case "ConsumableItem": return EItemType.ConsumableItem;
                default:
                    Debug.LogWarning($"Unknown ItemType: {typeStr}, defaulting to QuestItem");
                    return EItemType.QuestItem;
            }
        }

        /// <summary>
        /// Parse ItemEffect[] from string format: "ModifySanity:-15;ModifyHealth:10"
        /// </summary>
        private ItemEffect[] ParseItemEffects(string effectsStr)
        {
            if (string.IsNullOrWhiteSpace(effectsStr)) return new ItemEffect[0];

            var effects = new List<ItemEffect>();
            var effectPairs = effectsStr.Split(';');

            foreach (var pair in effectPairs)
            {
                var parts = pair.Split(':');
                if (parts.Length != 2) continue;

                var effectType = parts[0].Trim();
                if (!float.TryParse(parts[1].Trim(), out float value)) continue;

                var effect = new ItemEffect();
                effect.value = value;

                switch (effectType)
                {
                    case "ModifySanity":
                        effect.effectType = EItemEffectType.ModifySanity;
                        break;
                    case "ModifyHealth":
                        effect.effectType = EItemEffectType.ModifyHealth;
                        break;
                    default:
                        Debug.LogWarning($"Unknown effect type: {effectType}");
                        continue;
                }

                effects.Add(effect);
            }

            return effects.ToArray();
        }

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

        /// <summary>
        /// Creates the appropriate ScriptableObject type based on ObjectType from CSV.
        /// Only 5 types: Interactable, PhysicalDoor, Door, Bed, Item
        /// </summary>
        private ObjectDataSO CreateSOByType(string objectType)
        {
            switch (objectType.Trim())
            {
                case "Interactable":
                    return ScriptableObject.CreateInstance<InteractableDataSO>();

                case "PhysicalDoor":
                    return ScriptableObject.CreateInstance<PhysicalDoorDataSO>();

                case "Door":
                    return ScriptableObject.CreateInstance<DoorDataSO>();

                case "Bed":
                    return ScriptableObject.CreateInstance<BedDataSO>();

                case "Item":
                    return ScriptableObject.CreateInstance<ItemDataSO>();

                case "TriggerArea":
                case "Area":
                    return ScriptableObject.CreateInstance<AreaDataSO>();

                default:
                    Debug.LogError($"Unknown ObjectType: {objectType}. Please use: Interactable, PhysicalDoor, Door, Bed, Item, TriggerArea");
                    return ScriptableObject.CreateInstance<InteractableDataSO>(); // Fallback
            }
        }

        private class ObjectCSVRow
        {
            // Base ObjectDataSO fields
            public string ObjectID;
            public string ObjectType;
            public string RequiredFlags;
            public string HidingFlags;
            public string OnInteractTriggers;

            // ItemDataSO specific
            public string ItemType;
            public string ItemQuestKey;
            public string ItemPrefab;
            public string ItemEffects;
            public string DestroyOnPickup;

            // DoorDataSO specific
            public string NextSceneName;
            public string TargetSpawnID;

            // BedDataSO specific
            public string LoopableChapters;
            public string WakeUpSpawnID;
            public string CantSleepDialogue;

            // AreaDataSO specific
            public string OnEnterTriggers;
            public string OnExitTriggers;
            public string TriggerOnce;

            // Common
            public string Description;
        }
    }
}
#endif
