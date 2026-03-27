using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject data cho Item objects (collectibles).
    /// EXTENDS ObjectDataSO → Kế thừa common fields (flags, triggers, layers).
    /// Chỉ chứa item-specific fields: type, quest key, effects.
    /// </summary>
    [CreateAssetMenu(fileName = "ITEM_", menuName = "SO/Objects/ItemSO")]
    public class ItemDataSO : ObjectDataSO  // ✅ Changed from ScriptableObject to ObjectDataSO
    {
        // ==================== INHERITED FROM ObjectDataSO ====================
        // ✅ objectID
        // ✅ localizationKey
        // ✅ requiredFlags
        // ✅ hidingFlags
        // ✅ onInteractTriggers
        // ✅ interactableLayerName
        // ✅ inactiveLayerName
        // ✅ GetLocalizedName()
        // ✅ GetLocalizedDescription()

        // ==================== ITEM-SPECIFIC FIELDS ====================

        [Header("=== ITEM SPECIFIC ===")]
        [Tooltip("Prefab 3D của vật phẩm để sinh ra trong game")]
        public GameObject prefab;

        [Tooltip("Loại item: Key, Puzzle, Note, Quest, Consumable")]
        public EItemType itemType;

        [Header("=== PICKUP BEHAVIOR ===")]
        [Tooltip("Item biến mất sau khi pickup? (TRUE = Consumable/Collectible, FALSE = Decorative/Examinable)")]
        public bool destroyOnPickup = true;  // Default: Most items are consumable

        [Tooltip("Quest Key chung (VD: Cả sữa dâu và chuối đều điền 'QUEST_MILK')")]
        public string questKey;

        [Header("=== ITEM EFFECTS ===")]
        [Tooltip("Các hiệu ứng khi nhặt item (Hồi Sanity, Máu)")]
        public ItemEffect[] onPickupEffects;

        // ==================== LEGACY FALLBACK FIELDS (Optional) ====================
        // Giữ lại để backward compatible với old SOs
        [HideInInspector]
        public string itemID;  // Deprecated, use objectID instead

        [HideInInspector]
        public string itemName;  // Deprecated, use localizationKey instead

        [HideInInspector]
        [TextArea(4, 6)]
        public string description;  // Deprecated, use localizationKey_desc instead

        // ==================== MIGRATION HELPERS ====================

        /// <summary>
        /// Migrate old ItemDataSO data to new ObjectDataSO structure.
        /// Call this in Editor to auto-migrate legacy SOs.
        /// </summary>
        public void MigrateLegacyData()
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(itemID) && string.IsNullOrEmpty(objectID))
            {
                objectID = itemID;
                Debug.Log($"[ItemDataSO] Migrated itemID → objectID: {objectID}");
            }

            if (!string.IsNullOrEmpty(itemName) && string.IsNullOrEmpty(localizationKey))
            {
                localizationKey = itemID;  // Use itemID as localization key base
                Debug.Log($"[ItemDataSO] Migrated itemName → localizationKey: {localizationKey}");
            }

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public enum EItemType
    {
        Key,
        PuzzleItem,
        Note,
        QuestItem,
        ConsumableItem,
    }

    // ========== EFFECT SYSTEM ==========
    [System.Serializable]
    public class ItemEffect
    {
        public EItemEffectType effectType;
        public float value; // Số lượng thay đổi (+10 Sanity, -5 Health, v.v.)
    }

    public enum EItemEffectType
    {
        ModifySanity,   // Thay đổi Sanity (+/-)
        ModifyHealth,   // Thay đổi Health (+/-)
    }
}
