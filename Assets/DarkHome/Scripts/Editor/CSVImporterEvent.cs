#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DarkHome.Editor
{
    public class CSVImporterEvent : EditorWindow
    {
        private const string CSV_PATH = "Assets/DarkHome/Data/CSV/Events.csv";
        private const string OUTPUT_FOLDER = "Assets/DarkHome/SO/Resources";

        private string _csvPath = CSV_PATH;
        private string _outputFolder = OUTPUT_FOLDER;
        private string _chapterFilter = "Chapter1"; // Default chapter
        private bool _overwriteExisting = false;


        [MenuItem("Tools/DarkHome/Import Events CSV")]
        public static void ShowWindow()
        {
            GetWindow<CSVImporterEvent>("CSV Event Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("⚡ CSV Event Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            _csvPath = EditorGUILayout.TextField("CSV Path:", _csvPath);
            _outputFolder = EditorGUILayout.TextField("Output Folder:", _outputFolder);
            _chapterFilter = EditorGUILayout.TextField("Chapter Filter:", _chapterFilter);
            _overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing:", _overwriteExisting);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Imports Events.csv → EventSO.asset\n" +
                "Output: {OutputFolder}/{ChapterFilter}/EventSO.asset",
                MessageType.Info);
            EditorGUILayout.Space();

            GUI.backgroundColor = Color.green; // Changed from yellow for consistency
            if (GUILayout.Button("⚡ Import Events from CSV", GUILayout.Height(40)))
            {
                ImportEvents();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ImportEvents()
        {
            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", $"CSV not found: {_csvPath}", "OK");
                return;
            }

            List<EventCSVRow> rows = ParseCSV(_csvPath);
            if (rows.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No events in CSV", "OK");
                return;
            }

            string chapterFolder = Path.Combine(_outputFolder, _chapterFilter);
            EnsureFolderExists(chapterFolder);

            string fileName = "EventSO.asset";
            string fullPath = Path.Combine(chapterFolder, fileName);

            int created = 0, updated = 0, skipped = 0;
            EventTriggerDataSO eventSO;

            if (File.Exists(fullPath))
            {
                if (!_overwriteExisting)
                {
                    // Skip update - respect global toggle
                    skipped++;
                    Debug.LogWarning($"Skipped {fullPath} (already exists, overwrite disabled)");

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog("Info",
                        $"EventSO.asset already exists.\nEnable 'Overwrite Existing' to update.\n\nLocation: {fullPath}",
                        "OK");
                    return;
                }
                else
                {
                    // Update existing
                    eventSO = AssetDatabase.LoadAssetAtPath<EventTriggerDataSO>(fullPath);
                    Debug.Log($"Updating existing EventSO: {fullPath}");
                    updated++;
                }
            }
            else
            {
                // Create new
                eventSO = ScriptableObject.CreateInstance<EventTriggerDataSO>();
                AssetDatabase.CreateAsset(eventSO, fullPath);
                Debug.Log($"Created new EventSO: {fullPath}");
                created++;
            }

            eventSO.triggers = new List<EventTrigger>();

            // Convert CSV rows to EventTrigger
            foreach (var row in rows)
            {
                var trigger = new EventTrigger
                {
                    Id = row.EventID,
                    RequiredFlags = ParseFlags(row.RequiredFlags),
                    GrantedFlags = ParseFlags(row.GrantedFlags),
                    Type = ParseTriggerType(row.Type),
                    TargetObject = null, // Will be assigned manually in Inspector if needed
                    Data = row.Data
                };

                eventSO.triggers.Add(trigger);
            }

            EditorUtility.SetDirty(eventSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Import Complete!\n\n" +
                $"Created: {created}\n" +
                $"Updated: {updated}\n" +
                $"Skipped: {skipped}\n\n" +
                $"Total Events: {rows.Count}\n" +
                $"File: {fileName}\n\n" +
                $"Output: {fullPath}";

            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        // Parse CSV
        private List<EventCSVRow> ParseCSV(string path)
        {
            var rows = new List<EventCSVRow>();
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 6) continue;

                var row = new EventCSVRow
                {
                    EventID = parts[0].Trim(),
                    RequiredFlags = parts[1].Trim(),
                    GrantedFlags = parts[2].Trim(),
                    Type = parts[3].Trim(),
                    TargetObjectPath = parts[4].Trim(),
                    Data = parts[5].Trim()
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

        // Parse trigger type enum
        private ETriggerType ParseTriggerType(string typeStr)
        {
            if (System.Enum.TryParse<ETriggerType>(typeStr, true, out ETriggerType result))
            {
                return result;
            }

            Debug.LogWarning($"Unknown trigger type: {typeStr}, defaulting to FlagOnly");
            return ETriggerType.FlagOnly;
        }

        private void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // CSV Row class
        private class EventCSVRow
        {
            public string EventID;
            public string RequiredFlags;
            public string GrantedFlags;
            public string Type;
            public string TargetObjectPath;
            public string Data;
        }
    }
}
#endif
