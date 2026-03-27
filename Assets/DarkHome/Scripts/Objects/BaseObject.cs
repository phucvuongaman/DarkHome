using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public abstract class BaseObject : BaseInteractable
    {
        // ==================== NEW: SO-DRIVEN DATA ====================
        [Header("Data Source (NEW!)")]
        [Tooltip("ScriptableObject chứa tất cả config data (Flags, Triggers, Layers). Nếu có SO, Inspector fields bên dưới sẽ bị BỎ QUA.")]
        [SerializeField] protected ObjectDataSO _objectData;

        // ==================== LEGACY: INSPECTOR FIELDS (Backward Compatible) ====================
        [Header("Manual Config (Legacy - Không cần nếu có ObjectDataSO)")]
        [SerializeField] private List<FlagData> _requiredFlags; // Id để kiểm tra có bật tương tác.

        [Tooltip("Object will become INACTIVE (cannot interact) if the Player has ANY flag in this list.")]
        [SerializeField] private List<FlagData> _hidingFlags;

        [Tooltip("Danh sách ID của các EventTrigger sẽ được kích hoạt khi tương tác")]
        [SerializeField] protected List<FlagData> _onInteractTriggerIDs;

        [Header("Layer Settings")]
        [SerializeField] private string interactableLayerName = "Interactable";
        [SerializeField] private string inactiveLayerName = "Inactive";

        private int _interactableLayer;
        private int _inactiveLayer;

        public List<FlagData> RequiredFlags => _requiredFlags;
        // public List<FlagData> GrantedFlags => _grantedFlags;
        public List<FlagData> HidingFlags => _hidingFlags;

        // public string activateQuestID;
        // public string completeQuestID;

        public override InteractableType InteractType { get; set; }

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            //Load from ScriptableObject if assigned
            if (_objectData != null)
            {
                LoadFromSO();
            }
            else
            {
                // Debug.LogWarning($"[BaseObject] {name}: No ObjectDataSO assigned. Using Inspector values (legacy mode).", this);
            }

            // Chuyển tên layer thành số để hiệu quả hơn
            _interactableLayer = LayerMask.NameToLayer(interactableLayerName);
            _inactiveLayer = LayerMask.NameToLayer(inactiveLayerName);

            if (_interactableLayer == -1 || _inactiveLayer == -1)
            {
                // Debug.LogError($"Layer '{interactableLayerName}' or '{inactiveLayerName}' not found. Please define them in Project Settings -> Tags and Layers.", this);
            }
        }

        /// <summary>
        /// Load tất cả config data từ ObjectDataSO vào runtime cache.
        /// Override trong subclass nếu cần load thêm type-specific data.
        /// </summary>
        protected virtual void LoadFromSO()
        {
            if (_objectData == null) return;

            // Load common fields
            _requiredFlags = _objectData.requiredFlags;
            _hidingFlags = _objectData.hidingFlags;
            _onInteractTriggerIDs = _objectData.onInteractTriggers;
            interactableLayerName = _objectData.interactableLayerName;
            inactiveLayerName = _objectData.inactiveLayerName;

            // Override ID & Name
            Id = _objectData.objectID;
            InteractableName = _objectData.GetLocalizedName();

            // Debug.Log($" [BaseObject] {name}: Loaded data from SO '{_objectData.name}'");
        }

        protected virtual void Start()
        {
            ObjectManager.Instance.Register(this);
        }

        protected virtual void OnEnable()
        {

            EventManager.AddObserver<FlagData>(GameEvents.Flag.OnFlagChanged, HandleFlagChange);
            // Kiểm tra trạng thái lần đầu tiên khi object được bật
            UpdateInteractableState();
        }

        protected virtual void OnDisable()
        {
            // Hủy đăng ký để tránh lỗi
            EventManager.RemoveListener<FlagData>(GameEvents.Flag.OnFlagChanged, HandleFlagChange);
        }
        #endregion

        #region Interaction Logic
        // Hàm này được gọi bởi EventManager
        private void HandleFlagChange(FlagData changedFlag)
        {
            // Kiểm tra xem có bất kỳ flag nào thay đổi có nằm trong list yêu cầu không
            bool isRequiredFlag = RequiredFlags != null && RequiredFlags.Any(f => f.FlagID == changedFlag.FlagID);
            // Kiểm tra xem có bất kỳ flag nào thay đổi có nằm trong list ẩn không
            bool isHidingFlag = HidingFlags != null && HidingFlags.Any(f => f.FlagID == changedFlag.FlagID);
            if (isRequiredFlag || isHidingFlag)
            {
                UpdateInteractableState();
            }
        }

        // Hàm này tự động cập nhật layer của object
        private void UpdateInteractableState()
        {
            // Kiểm trá xem có đủ các flag yêu cầu chưa
            bool hasRequired = FlagManager.Instance.HasAllFlags(RequiredFlags);
            // Kiểm tra xem có 1 trong các flag ẩn nào chưa
            bool hasHiding = FlagManager.Instance.HasAnyFlag(HidingFlags);
            // nếu có bất kỳ yêu cầu nào chưa có hoặc có bất kỳ flag ẩn nào thì sẽ không cho interact
            bool canInteract = hasRequired && !hasHiding;
            int targetLayer = canInteract ? _interactableLayer : _inactiveLayer;

            if (gameObject.layer != targetLayer && targetLayer != -1)
            {
                gameObject.layer = targetLayer;
            }

            // Gọi hàm ảo để các lớp con (như Door) có thể thêm hành vi riêng (khóa/mở)
            OnInteractableStateChanged(canInteract);
        }

        // Hàm ảo cho các lớp con
        protected virtual void OnInteractableStateChanged(bool canInteract) { }

        // Hàm tương tác mặc định
        public override void OnInteractPress(Interactor interactor)
        {
            ActivateTriggers(_onInteractTriggerIDs);
        }

        // Hàm tiện ích để kích hoạt event
        protected void ActivateTriggers(List<FlagData> triggerIdList)
        {
            if (triggerIdList != null)
            {
                foreach (var triggerId in triggerIdList)
                {
                    // Debug.Log($"[BaseObject] {name}: Activating trigger {triggerId.FlagID}");
                    EventTriggerManager.Instance.ActiveEvent(triggerId);
                }
            }
        }
        #endregion


        #region Save/Load Data
        public ObjectData GetCurrentObjectData()
        {
            return new ObjectData
            {
                objId = Id,
                position = transform.position,
                rotation = transform.rotation,
                isActive = gameObject.activeSelf,
            };
        }

        public void LoadData(ObjectData data)
        {
            if (data == null) return;
            transform.position = data.position;
            transform.rotation = data.rotation;
            gameObject.SetActive(data.isActive);
        }
        #endregion
    }


}