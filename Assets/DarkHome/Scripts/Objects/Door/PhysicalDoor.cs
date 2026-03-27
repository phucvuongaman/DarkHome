using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Physical door for in-scene room transitions (NO scene loading).
    /// Uses HingeJoint for physics-based door opening.
    /// Checks RequiredFlags from ObjectDataSO to lock/unlock.
    /// 
    /// Usage: Bedroom <-> Living Room (same scene)
    /// VS Door.cs which loads different scenes
    /// </summary>
    [RequireComponent(typeof(HingeJoint))]
    public class PhysicalDoor : BaseObject
    {
        [Header("=== PHYSICAL DOOR ===")]
        [SerializeField] private HingeJoint _hingeJoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _lockedSound;
        [SerializeField] private AudioClip _unlockSound;

        private bool _isLocked = true;
        private bool _hasPlayedUnlockSound = false; // Track if unlock sound already played

        public override InteractableType InteractType => InteractableType.Door;

        /// <summary>
        /// Override để load PhysicalDoor-specific data từ PhysicalDoorDataSO.
        /// (Hiện tại không có specific fields, chỉ dùng base ObjectDataSO)
        /// </summary>
        protected override void LoadFromSO()
        {
            base.LoadFromSO();  // Load common fields (objectID, localizationKey, requiredFlags, onInteractTriggers)

            // PhysicalDoorDataSO không có thêm fields nào
            // HingeJoint và AudioClips setup trực tiếp trong Inspector

            // if (_objectData is PhysicalDoorDataSO)
            // {
            //     Debug.Log($" [PhysicalDoor] {name}: Loaded PhysicalDoorDataSO - RequiredFlags: {RequiredFlags?.Count ?? 0}");
            // }
        }

        protected override void Awake()
        {
            base.Awake();

            if (_hingeJoint == null)
                _hingeJoint = GetComponent<HingeJoint>();

            // Disable hinge initially (locked)
            if (_hingeJoint != null)
                _hingeJoint.useMotor = false;
        }

        protected override void Start()
        {
            base.Start();
            CheckLockState();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.AddObserver<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        public override void OnInteractPress(Interactor interactor)
        {
            // Fire onInteractTriggers from ObjectDataSO
            base.OnInteractPress(interactor);

            // Check if door can be unlocked NOW (on player interaction)
            bool canUnlock = (RequiredFlags == null || RequiredFlags.Count == 0) ||
                             (FlagManager.Instance != null && FlagManager.Instance.HasAllFlags(RequiredFlags));

            if (!canUnlock)
            {
                // Still locked - play locked sound
                PlaySound(_lockedSound);
                // Debug.Log($" [PhysicalDoor] {Id}: Still locked");
            }
            else if (_isLocked)
            {
                // First time unlocking on THIS interaction - play unlock sound ONCE
                UnlockDoor();
                PlaySound(_unlockSound);
                // Debug.Log($"[PhysicalDoor] {Id}: Unlocked!");
            }
            else
            {
                // Already unlocked - no sound, just let physics handle push
                // Debug.Log($" [PhysicalDoor] {Id}: Already unlocked - player can push");
            }
        }

        /// <summary>
        /// Check if door should be locked based on RequiredFlags
        /// </summary>
        private void CheckLockState()
        {
            if (RequiredFlags == null || RequiredFlags.Count == 0)
            {
                // No requirements - unlocked by default
                UnlockDoor();
                return;
            }

            // Check if all required flags are met
            if (FlagManager.Instance != null && FlagManager.Instance.HasAllFlags(RequiredFlags))
            {
                UnlockDoor();
            }
            else
            {
                LockDoor();
            }
        }

        /// <summary>
        /// Handle flag changes - check if door should unlock
        /// </summary>
        private void OnFlagChanged(FlagData changedFlag)
        {
            CheckLockState();
        }

        private void UnlockDoor()
        {
            if (!_isLocked) return;

            _isLocked = false;

            // Enable HingeJoint so player can push door
            if (_hingeJoint != null)
                _hingeJoint.useMotor = false; // Let physics handle it

            // Debug.Log($"[PhysicalDoor] {Id}: Unlocked (internal state)");
        }

        private void LockDoor()
        {
            if (_isLocked) return;

            _isLocked = true;

            // Disable HingeJoint
            if (_hingeJoint != null)
                _hingeJoint.useMotor = false;

            // Debug.Log($"[PhysicalDoor] {Id}: Locked");
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }

        protected override void OnInteractableStateChanged(bool canInteract)
        {
            // Optional: Visual feedback when door becomes interactable
        }
    }
}
