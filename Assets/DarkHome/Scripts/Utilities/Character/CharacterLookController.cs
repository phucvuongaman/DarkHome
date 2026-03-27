using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Unified target manager for HeadLook + VRoidEyeTracker
    /// Synchronizes targets and handles priority contexts (dialogue, cutscene, idle)
    /// </summary>
    public class CharacterLookController : MonoBehaviour
    {
        [Header("Components (Auto-detect if null)")]
        [SerializeField] private HeadLook _headLook;
        [SerializeField] private VRoidEyeTracker _eyeTracker;

        [Header("Target Priority")]
        [Tooltip("Current look target (highest priority override)")]
        private Transform _currentTarget;

        [Tooltip("Default target when no override (usually null for NPC, can be Camera for menu characters)")]
        [SerializeField] private Transform _defaultTarget;

        [Header("Behavior Settings")]
        [Tooltip("Enable/disable head tracking")]
        [SerializeField] private bool _enableHeadTracking = true;

        [Tooltip("Enable/disable eye tracking")]
        [SerializeField] private bool _enableEyeTracking = true;

        [Header("Context Settings")]
        [Tooltip("Disable all tracking during cutscenes")]
        [SerializeField] private bool _disableDuringCutscenes = true;

        private bool _isCutscenePlaying = false;

        private void Awake()
        {
            // Auto-detect components
            if (_headLook == null)
                _headLook = GetComponent<HeadLook>();

            if (_eyeTracker == null)
                _eyeTracker = GetComponent<VRoidEyeTracker>();
        }

        private void Start()
        {
            // Initialize with default target
            SetTarget(_defaultTarget);
        }

        private void Update()
        {
            // Update component targets
            SyncTargets();

            // Update component enable states
            UpdateComponentStates();
        }

        /// <summary>
        /// Set look target (high priority - used for dialogue, interactions)
        /// </summary>
        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }

        /// <summary>
        /// Clear target, return to default
        /// </summary>
        public void ClearTarget()
        {
            _currentTarget = _defaultTarget;
        }

        /// <summary>
        /// Enable/disable specific tracking components
        /// </summary>
        public void SetHeadTrackingEnabled(bool enabled)
        {
            _enableHeadTracking = enabled;
        }

        public void SetEyeTrackingEnabled(bool enabled)
        {
            _enableEyeTracking = enabled;
        }

        /// <summary>
        /// Called by cutscene system to disable all tracking
        /// </summary>
        public void OnCutsceneStart()
        {
            _isCutscenePlaying = true;

            if (_disableDuringCutscenes)
            {
                // Disable all tracking during cutscene
                if (_headLook != null)
                    _headLook.enabled = false;

                if (_eyeTracker != null)
                    _eyeTracker.enabled = false;
            }
        }

        /// <summary>
        /// Called by cutscene system to re-enable tracking
        /// </summary>
        public void OnCutsceneEnd()
        {
            _isCutscenePlaying = false;
            UpdateComponentStates();
        }

        /// <summary>
        /// For dialogue system: look at player
        /// </summary>
        public void OnDialogueStart(Transform player)
        {
            SetTarget(player);
        }

        /// <summary>
        /// For dialogue system: return to default behavior
        /// </summary>
        public void OnDialogueEnd()
        {
            ClearTarget();
        }

        private void SyncTargets()
        {
            // Sync targets to components
            if (_headLook != null)
                _headLook.Target = _currentTarget;

            if (_eyeTracker != null)
                _eyeTracker.target = _currentTarget;
        }

        private void UpdateComponentStates()
        {
            // Don't enable during cutscene if disabled
            if (_isCutscenePlaying && _disableDuringCutscenes)
                return;

            // Update HeadLook state
            if (_headLook != null)
                _headLook.enabled = _enableHeadTracking;

            // Update EyeTracker state
            if (_eyeTracker != null)
                _eyeTracker.enabled = _enableEyeTracking;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Print Current Target")]
        private void DebugPrintTarget()
        {
            Debug.Log($"[CharacterLookController] Current Target: {(_currentTarget != null ? _currentTarget.name : "NULL")}");
            Debug.Log($"Head Tracking: {_enableHeadTracking}, Eye Tracking: {_enableEyeTracking}");
            Debug.Log($"In Cutscene: {_isCutscenePlaying}");
        }
#endif
    }
}
