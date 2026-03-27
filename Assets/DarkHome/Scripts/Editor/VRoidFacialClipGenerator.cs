// Deleted
// Test script do AI tạo ra.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DarkHome
{
    /// <summary>
    /// Auto-generate facial animation clips for VRoid characters
    /// Menu: Tools → VRoid → Generate Facial Clips
    /// </summary>
    public class VRoidFacialClipGenerator : EditorWindow
    {
        private GameObject _targetCharacter;
        private string _outputFolder = "Assets/DarkHome/Animation/Clip/Facial";

        [MenuItem("Tools/VRoid/Generate Facial Clips")]
        public static void ShowWindow()
        {
            var window = GetWindow<VRoidFacialClipGenerator>("VRoid Facial Generator");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("🎭 VRoid Facial Clip Generator", EditorStyles.largeLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool auto-generates facial animation clips for VRoid characters.\n\n" +
                "1. Drag your VRoid character prefab/scene object below\n" +
                "2. Choose output folder\n" +
                "3. Click Generate!",
                MessageType.Info
            );

            GUILayout.Space(10);

            // Target character
            _targetCharacter = EditorGUILayout.ObjectField(
                "VRoid Character:",
                _targetCharacter,
                typeof(GameObject),
                true
            ) as GameObject;

            // Output folder
            EditorGUILayout.BeginHorizontal();
            _outputFolder = EditorGUILayout.TextField("Output Folder:", _outputFolder);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Convert absolute path to Unity Assets-relative path
                    if (path.StartsWith(Application.dataPath))
                    {
                        _outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        Debug.LogWarning("Selected folder is outside the Assets folder! Please select a folder inside Assets.");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Generate button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🎬 Generate Facial Clips", GUILayout.Height(40)))
            {
                GenerateClips();
            }
            GUI.backgroundColor = Color.white;
        }

        private void GenerateClips()
        {
            // Validate
            if (_targetCharacter == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a VRoid character!", "OK");
                return;
            }

            // Find SkinnedMeshRenderer (Face/Body)
            SkinnedMeshRenderer face = _targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>();
            if (face == null)
            {
                EditorUtility.DisplayDialog("Error", "No SkinnedMeshRenderer found on character!", "OK");
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

            // Get relative path to face
            string facePath = GetRelativePath(_targetCharacter.transform, face.transform);

            // Define expressions with VRoid standard BlendShape names
            var expressions = new System.Collections.Generic.Dictionary<string, string>
            {
                {"Neutral", ""},  // All zeros
                {"Happy", "Fcl_ALL_Joy"},
                {"Sad", "Fcl_ALL_Sorrow"},
                {"Angry", "Fcl_ALL_Angry"},
                {"Surprised", "Fcl_ALL_Surprised"},
                {"Fun", "Fcl_ALL_Fun"}
            };

            int count = 0;

            // Generate each expression clip
            foreach (var expr in expressions)
            {
                AnimationClip clip = CreateExpressionClip(facePath, expr.Value, face.sharedMesh);

                if (clip != null)
                {
                    string clipPath = $"{_outputFolder}/{expr.Key}.anim";
                    AssetDatabase.CreateAsset(clip, clipPath);
                    count++;
                    Debug.Log($"✅ Created: {clipPath}");
                }
            }

            // Generate Blink clip
            AnimationClip blinkClip = CreateBlinkClip(facePath, face.sharedMesh);
            if (blinkClip != null)
            {
                string blinkPath = $"{_outputFolder}/Blink.anim";
                AssetDatabase.CreateAsset(blinkClip, blinkPath);
                count++;
                Debug.Log($"✅ Created: {blinkPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Success!",
                $"Generated {count} facial animation clips!\n\n" +
                $"Location: {_outputFolder}\n\n" +
                "Next step: Create Animator Controller and add these clips to states.",
                "OK"
            );
        }

        private AnimationClip CreateExpressionClip(string facePath, string blendShapeName, Mesh mesh)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = false;

            if (string.IsNullOrEmpty(blendShapeName))
            {
                // Neutral - reset all
                ResetAllBlendShapes(clip, facePath, mesh);
            }
            else
            {
                // Check if blend shape exists
                int index = mesh.GetBlendShapeIndex(blendShapeName);
                if (index < 0)
                {
                    Debug.LogWarning($"BlendShape '{blendShapeName}' not found! Skipping...");
                    return null;
                }

                // Reset all first
                ResetAllBlendShapes(clip, facePath, mesh);

                // Set target expression to 100
                AnimationCurve curve = AnimationCurve.Constant(0f, 0f, 100f);
                clip.SetCurve(facePath, typeof(SkinnedMeshRenderer), $"blendShape.{blendShapeName}", curve);
            }

            return clip;
        }

        private AnimationClip CreateBlinkClip(string facePath, Mesh mesh)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = false;

            // Find blink BlendShape
            string blinkName = FindBlinkBlendShape(mesh);
            if (string.IsNullOrEmpty(blinkName))
            {
                Debug.LogWarning("Blink BlendShape not found!");
                return null;
            }

            // Create blink animation (0 → 100 → 0)
            Keyframe[] keys = new Keyframe[]
            {
                new Keyframe(0.0f, 0f),
                new Keyframe(0.1f, 100f),
                new Keyframe(0.2f, 0f)
            };

            AnimationCurve curve = new AnimationCurve(keys);
            clip.SetCurve(facePath, typeof(SkinnedMeshRenderer), $"blendShape.{blinkName}", curve);

            return clip;
        }

        private void ResetAllBlendShapes(AnimationClip clip, string facePath, Mesh mesh)
        {
            // Common VRoid facial BlendShapes to reset
            string[] commonShapes = new string[]
            {
                "Fcl_ALL_Joy", "Fcl_ALL_Angry", "Fcl_ALL_Sorrow",
                "Fcl_ALL_Surprised", "Fcl_ALL_Fun"
            };

            AnimationCurve zeroCurve = AnimationCurve.Constant(0f, 0f, 0f);

            foreach (string shapeName in commonShapes)
            {
                if (mesh.GetBlendShapeIndex(shapeName) >= 0)
                {
                    clip.SetCurve(facePath, typeof(SkinnedMeshRenderer), $"blendShape.{shapeName}", zeroCurve);
                }
            }
        }

        private string FindBlinkBlendShape(Mesh mesh)
        {
            // Try common blink BlendShape names
            string[] candidates = new string[]
            {
                "Fcl_EYE_Close", "Fcl_ALL_Blink", "Blink", "blink"
            };

            foreach (string name in candidates)
            {
                if (mesh.GetBlendShapeIndex(name) >= 0)
                    return name;
            }

            return null;
        }

        private string GetRelativePath(Transform root, Transform target)
        {
            if (root == target)
                return "";

            string path = target.name;
            Transform current = target.parent;

            while (current != null && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
#endif
