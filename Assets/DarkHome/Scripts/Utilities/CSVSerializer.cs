using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace DarkHome
{
    /// <summary>
    /// GENERIC CSV SERIALIZER - Công cụ chuyển đổi SO ↔ CSV
    /// 
    /// SỬ DỤNG:
    /// - Export: CSVSerializer.ExportToCSV<ItemDataSO>(items, "Items.csv")
    /// - Import: CSVSerializer.ImportFromCSV<ItemDataSO>("Items.csv")
    /// 
    /// HỖ TRỢ:
    /// - Primitive types (int, float, string, bool, enum)
    /// - Lists/Arrays
    /// - Special handling cho Sprite, GameObject references
    /// </summary>
    public static class CSVSerializer
    {
#if UNITY_EDITOR
        /// <summary>
        /// Export danh sách SO ra file CSV
        /// </summary>
        public static void ExportToCSV<T>(List<T> objects, string csvPath) where T : ScriptableObject
        {
            if (objects == null || objects.Count == 0)
            {
                Debug.LogWarning("No objects to export!");
                return;
            }

            var sb = new StringBuilder();
            var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            // ===== HEADER ROW =====
            sb.Append("AssetPath,"); // Cột đầu: path của SO file
            foreach (var field in fields)
            {
                // Skip fields không serialize được
                if (field.IsNotSerialized) continue;
                if (IsComplexType(field.FieldType)) continue; // Skip List<>, custom classes...

                sb.Append(field.Name);
                sb.Append(",");
            }
            sb.AppendLine();

            // ===== DATA ROWS =====
            foreach (var obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                sb.Append($"\"{assetPath}\",");

                foreach (var field in fields)
                {
                    if (field.IsNotSerialized) continue;
                    if (IsComplexType(field.FieldType)) continue;

                    object value = field.GetValue(obj);
                    string csvValue = ConvertToCSVValue(value, field.FieldType);
                    sb.Append(csvValue);
                    sb.Append(",");
                }
                sb.AppendLine();
            }

            // Write to file
            string directory = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            // Debug.Log($"Exported {objects.Count} items to: {csvPath}");
        }

        /// <summary>
        /// Import CSV và update hoặc tạo mới SO files
        /// </summary>
        public static List<T> ImportFromCSV<T>(string csvPath, string soOutputFolder) where T : ScriptableObject
        {
            if (!File.Exists(csvPath))
            {
                Debug.LogError($"CSV file not found: {csvPath}");
                return null;
            }

            var results = new List<T>();
            string[] lines = File.ReadAllLines(csvPath);

            if (lines.Length < 2)
            {
                Debug.LogWarning("CSV file is empty or missing data!");
                return results;
            }

            // Parse header
            string[] headers = SplitCSVLine(lines[0]);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = SplitCSVLine(line);
                if (values.Length != headers.Length)
                {
                    Debug.LogWarning($"Row {i}: Column count mismatch. Skipping.");
                    continue;
                }

                // Kiểm tra SO đã tồn tại chưa
                string assetPath = values[0];
                T so;

                if (!string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
                {
                    // Load existing SO
                    so = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
                else
                {
                    // Tạo mới SO
                    so = ScriptableObject.CreateInstance<T>();
                    string soName = $"{typeof(T).Name}_{i}.asset";
                    string newPath = Path.Combine(soOutputFolder, soName);
                    AssetDatabase.CreateAsset(so, newPath);
                }

                // Apply values from CSV
                var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                for (int j = 1; j < headers.Length && j < values.Length; j++)
                {
                    string fieldName = headers[j];
                    string value = values[j];

                    var field = fields.FirstOrDefault(f => f.Name == fieldName);
                    if (field == null) continue;
                    if (IsComplexType(field.FieldType)) continue;

                    object convertedValue = ConvertFromCSVValue(value, field.FieldType);
                    field.SetValue(so, convertedValue);
                }

                EditorUtility.SetDirty(so);
                results.Add(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Debug.Log($"Imported {results.Count} items from CSV");
            return results;
        }

        // ==================== HELPER METHODS ====================

        private static bool IsComplexType(Type type)
        {
            // Check if type is List, Array, or custom class
            if (type.IsArray) return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return true;
            if (type == typeof(Sprite) || type == typeof(GameObject)) return false; // Handle separately
            if (!type.IsPrimitive && !type.IsEnum && type != typeof(string)) return true;
            return false;
        }

        private static string ConvertToCSVValue(object value, Type fieldType)
        {
            if (value == null) return "\"\"";

            // Handle UnityEngine.Object references (Sprite, Prefab...)
            if (value is UnityEngine.Object unityObj)
            {
                string assetPath = AssetDatabase.GetAssetPath(unityObj);
                return $"\"{assetPath}\"";
            }

            // Handle strings with commas/quotes
            if (fieldType == typeof(string))
            {
                string str = value.ToString();
                str = str.Replace("\"", "\"\""); // Escape quotes
                return $"\"{str}\"";
            }

            // Handle enums
            if (fieldType.IsEnum)
            {
                return value.ToString();
            }

            // Primitive types
            return value.ToString();
        }

        private static object ConvertFromCSVValue(string csvValue, Type fieldType)
        {
            // Remove surrounding quotes
            if (csvValue.StartsWith("\"") && csvValue.EndsWith("\""))
            {
                csvValue = csvValue.Substring(1, csvValue.Length - 2);
                csvValue = csvValue.Replace("\"\"", "\""); // Unescape quotes
            }

            if (string.IsNullOrEmpty(csvValue)) return GetDefaultValue(fieldType);

            // Handle UnityEngine.Object references
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return AssetDatabase.LoadAssetAtPath(csvValue, fieldType);
            }

            // Handle enums
            if (fieldType.IsEnum)
            {
                return Enum.Parse(fieldType, csvValue);
            }

            // Handle primitives
            return Convert.ChangeType(csvValue, fieldType);
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Split CSV line, handling quotes properly
        /// </summary>
        private static string[] SplitCSVLine(string line)
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
#endif
    }
}
