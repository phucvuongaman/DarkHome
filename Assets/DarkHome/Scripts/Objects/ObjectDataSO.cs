using UnityEngine;
using System.Collections.Generic;

namespace DarkHome
{
    /// <summary>
    /// Base ScriptableObject cho TẤT CẢ objects có thể tương tác.
    /// Chứa các fields chung: Flags, Triggers, Layers.
    /// Subclasses: ItemDataSO, DoorDataSO, BedDataSO, AreaDataSO, PuzzleDataSO
    /// </summary>
    public abstract class ObjectDataSO : ScriptableObject
    {
        [Header("=== CORE IDENTITY ===")]
        [Tooltip("Unique ID cho object (VD: ITEM_MILK, DOOR_OFFICE, BED_PLAYER)")]
        public string objectID;

        [Header("=== LOCALIZATION ===")]
        [Tooltip("Base key cho localization (VD: ITEM_MILK, DOOR_OFFICE). Sẽ append '_name' hoặc '_desc'.")]
        public string localizationKey;

        [Header("=== INTERACTION REQUIREMENTS ===")]
        [Tooltip("Player BẮT BUỘC phải có TẤT CẢ các flags này để tương tác được")]
        public List<FlagData> requiredFlags = new List<FlagData>();

        [Tooltip("Nếu Player có BẤT KỲ flag nào trong này, object sẽ bị ẨN (không thể tương tác)")]
        public List<FlagData> hidingFlags = new List<FlagData>();

        [Header("=== EVENT TRIGGERS ===")]
        [Tooltip("Danh sách Event Triggers sẽ được kích hoạt khi tương tác với object")]
        public List<FlagData> onInteractTriggers = new List<FlagData>();

        [Header("=== LAYER SETTINGS ===")]
        [Tooltip("Tên layer khi object có thể tương tác (mặc định: Interactable)")]
        public string interactableLayerName = "Interactable";

        [Tooltip("Tên layer khi object KHÔNG thể tương tác (mặc định: Inactive)")]
        public string inactiveLayerName = "Inactive";


        /// <summary>
        /// Lấy tên Object đã được localize.
        /// Convention: objectID chính là TextKey trong VN.csv / EN.csv
        /// VD: TV_LIVING -> LocalizationManager.GetText("TV_LIVING") -> "Ti Vi"
        /// Fallback: objectID nếu không tìm thấy trong localization
        /// </summary>
        public virtual string GetLocalizedName()
        {
            if (LocalizationManager.Instance == null)
            {
                return objectID; // Fallback if no manager
            }

            // Use objectID as TextKey directly
            string localizedName = LocalizationManager.Instance.GetText(objectID);

            // If key not found, LocalizationManager returns the key itself
            // So we just return it (either localized text or objectID)
            return localizedName;
        }

        /// <summary>
        /// Lấy description đã được localize.
        /// Pattern: LocalizationKey → LocalizationKey_desc
        /// </summary>
        public virtual string GetLocalizedDescription()
        {
            if (string.IsNullOrEmpty(localizationKey) || LocalizationManager.Instance == null)
            {
                return ""; // Fallback
            }

            return LocalizationManager.Instance.GetText(localizationKey + "_desc");
        }
    }
}
