using System.Collections.Generic;
using UnityEngine;


namespace DarkHome
{
    [System.Serializable]
    public class Choice
    {
        public string ChoiceId;

        [Tooltip("DEPRECATED: Legacy direct text. Use ChoiceTextKey for localization instead.")]
        [TextArea(4, 6)]
        public string ChoiceText;

        [Tooltip("Localization key for this choice text (e.g. CHOICE_TELL_DREAM)")]
        public string ChoiceTextKey;

        public string NextNodeID;
        public bool IsHidden; // Này để debug ban đầu ẩn choice các thứ
        [Tooltip("Player BẮT BUỘC phải có TẤT CẢ các cờ này để THẤY choice")]
        public List<FlagData> RequiredFlags;

        [Tooltip("Nếu Player có BẤT KỲ cờ nào trong này, choice sẽ bị ẨN")]
        public List<FlagData> HidingFlags;

        [Tooltip("Event triggers kích hoạt khi chọn choice này (e.g. EVENT_ACTIVATE_QUEST, EVENT_TRACK_FLAG)")]
        public List<FlagData> OnSelectTriggers;

        /// <summary>
        /// Gets the localized choice text.
        /// If ChoiceTextKey is set, uses LocalizationManager.
        /// Otherwise, falls back to legacy ChoiceText field.
        /// </summary>
        public string GetText()
        {
            // New way: Use localization key
            if (!string.IsNullOrEmpty(ChoiceTextKey))
            {
                return LocalizationManager.Instance.GetText(ChoiceTextKey);
            }

            // Legacy way: Use direct text (for old SOs)
            return ChoiceText;
        }
    }
}