using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Eye Tracking for VRoid characters
    /// Automatically looks at target (player, object, etc.)
    /// </summary>
    public class VRoidEyeTracker : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("Target to look at (e.g., Main Camera, Player, etc.)")]
        public Transform target;

        [Tooltip("Auto-find Main Camera if target is null")]
        public bool autoFindCamera = true;

        [Header("Eye Settings")]
        [Tooltip("Face SkinnedMeshRenderer (auto-detect if null)")]
        public SkinnedMeshRenderer faceRenderer;

        [Range(0f, 100f)]
        [Tooltip("Maximum eye movement range")]
        public float maxEyeMovement = 50f;

        [Range(0f, 1f)]
        [Tooltip("Eye movement smoothness (0 = instant, 1 = very smooth)")]
        public float smoothness = 0.1f;

        [Header("Limits")]
        [Tooltip("Enable horizontal eye movement")]
        public bool enableHorizontal = true;

        [Tooltip("Enable vertical eye movement")]
        public bool enableVertical = true;

        [Range(0f, 90f)]
        [Tooltip("Maximum horizontal angle (degrees)")]
        public float maxHorizontalAngle = 45f;

        [Range(0f, 90f)]
        [Tooltip("Maximum vertical angle (degrees)")]
        public float maxVerticalAngle = 30f;

        // BlendShape indices (cached for performance)
        private int _lookLeftIndex = -1;
        private int _lookRightIndex = -1;
        private int _lookUpIndex = -1;
        private int _lookDownIndex = -1;

        // Current eye positions (for smoothing)
        private float _currentHorizontal = 0f;
        private float _currentVertical = 0f;

        private void Start()
        {
            // Auto-detect face renderer
            if (faceRenderer == null)
            {
                faceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (faceRenderer == null)
                {
                    Debug.LogError("[VRoidEyeTracker] No SkinnedMeshRenderer found!");
                    enabled = false;
                    return;
                }
            }

            // Auto-find camera
            if (target == null && autoFindCamera)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    target = mainCam.transform;
                    Debug.Log("[VRoidEyeTracker] Auto-assigned Main Camera as target");
                }
            }

            // Cache BlendShape indices
            CacheBlendShapeIndices();
        }

        private void CacheBlendShapeIndices()
        {
            Mesh mesh = faceRenderer.sharedMesh;

            // Try common VRoid eye BlendShape names
            _lookLeftIndex = FindBlendShapeIndex(mesh, "Fcl_EYE_Look_Left", "Eye_Look_L", "EyeLeft");
            _lookRightIndex = FindBlendShapeIndex(mesh, "Fcl_EYE_Look_Right", "Eye_Look_R", "EyeRight");
            _lookUpIndex = FindBlendShapeIndex(mesh, "Fcl_EYE_Look_Up", "Eye_Look_U", "EyeUp");
            _lookDownIndex = FindBlendShapeIndex(mesh, "Fcl_EYE_Look_Down", "Eye_Look_D", "EyeDown");

            // Debug log
            if (_lookLeftIndex < 0 || _lookRightIndex < 0)
            {
                Debug.LogWarning("[VRoidEyeTracker] Horizontal eye BlendShapes not found!");
            }
            if (_lookUpIndex < 0 || _lookDownIndex < 0)
            {
                Debug.LogWarning("[VRoidEyeTracker] Vertical eye BlendShapes not found!");
            }
        }

        private int FindBlendShapeIndex(Mesh mesh, params string[] names)
        {
            foreach (string name in names)
            {
                int index = mesh.GetBlendShapeIndex(name);
                if (index >= 0)
                    return index;
            }
            return -1;
        }

        private void LateUpdate()
        {
            if (target == null || faceRenderer == null)
                return;

            // Calculate direction to target
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.Normalize();

            // Convert to local space
            Vector3 localDirection = transform.InverseTransformDirection(directionToTarget);

            // Calculate angles
            float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

            // Clamp angles
            horizontalAngle = Mathf.Clamp(horizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
            verticalAngle = Mathf.Clamp(verticalAngle, -maxVerticalAngle, maxVerticalAngle);

            // Normalize to 0-1 range
            float normalizedH = horizontalAngle / maxHorizontalAngle; // -1 to 1
            float normalizedV = verticalAngle / maxVerticalAngle; // -1 to 1

            // Calculate target BlendShape values
            float targetH = normalizedH * maxEyeMovement;
            float targetV = normalizedV * maxEyeMovement;

            // Smooth interpolation
            _currentHorizontal = Mathf.Lerp(_currentHorizontal, targetH, 1f - smoothness);
            _currentVertical = Mathf.Lerp(_currentVertical, targetV, 1f - smoothness);

            // Apply horizontal
            if (enableHorizontal)
            {
                if (_currentHorizontal > 0 && _lookRightIndex >= 0)
                {
                    faceRenderer.SetBlendShapeWeight(_lookRightIndex, _currentHorizontal);
                    if (_lookLeftIndex >= 0)
                        faceRenderer.SetBlendShapeWeight(_lookLeftIndex, 0f);
                }
                else if (_currentHorizontal < 0 && _lookLeftIndex >= 0)
                {
                    faceRenderer.SetBlendShapeWeight(_lookLeftIndex, -_currentHorizontal);
                    if (_lookRightIndex >= 0)
                        faceRenderer.SetBlendShapeWeight(_lookRightIndex, 0f);
                }
            }

            // Apply vertical
            if (enableVertical)
            {
                if (_currentVertical > 0 && _lookUpIndex >= 0)
                {
                    faceRenderer.SetBlendShapeWeight(_lookUpIndex, _currentVertical);
                    if (_lookDownIndex >= 0)
                        faceRenderer.SetBlendShapeWeight(_lookDownIndex, 0f);
                }
                else if (_currentVertical < 0 && _lookDownIndex >= 0)
                {
                    faceRenderer.SetBlendShapeWeight(_lookDownIndex, -_currentVertical);
                    if (_lookUpIndex >= 0)
                        faceRenderer.SetBlendShapeWeight(_lookUpIndex, 0f);
                }
            }
        }

        private void OnDisable()
        {
            // Reset eyes to neutral
            if (faceRenderer != null)
            {
                if (_lookLeftIndex >= 0) faceRenderer.SetBlendShapeWeight(_lookLeftIndex, 0f);
                if (_lookRightIndex >= 0) faceRenderer.SetBlendShapeWeight(_lookRightIndex, 0f);
                if (_lookUpIndex >= 0) faceRenderer.SetBlendShapeWeight(_lookUpIndex, 0f);
                if (_lookDownIndex >= 0) faceRenderer.SetBlendShapeWeight(_lookDownIndex, 0f);
            }
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(target.position, 0.1f);
            }
        }
    }
}
