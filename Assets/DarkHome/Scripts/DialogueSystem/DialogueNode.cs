using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    [System.Serializable]
    public class DialogueNode
    {
        public string NodeId;
        public string IdSpeaker;

        [Header("Text Content")]
        [Tooltip("DEPRECATED: Legacy direct text. Use DialogueTextKey for localization instead.")]
        [TextArea(4, 6)]
        public string DialogueText;

        [Tooltip("Localization key for this dialogue text (e.g. DIALOGUE_KAI_GREET_START)")]
        public string DialogueTextKey;
        public string NextId;

        [Header("Auto Flags")]
        [Tooltip("Flags granted automatically when this dialogue node is displayed (not requiring Choice)")]
        public List<FlagData> GrantedFlags;


        [Header("Logic Conditions")]
        [Tooltip("Danh sách các lựa chọn mà người chơi có thể đưa ra sau câu thoại này.")]
        public List<Choice> Choices;

        [Tooltip("Node khởi đầu DUY NHẤT cho cuộc trò chuyện LẦN ĐẦU TIÊN.")]
        public bool IsStartNode;

        [Tooltip("Các cờ bắt buộc người chơi phải có để nghe được câu thoại này.")]
        public List<FlagData> RequiredFlags;

        [Tooltip("Độ ưu tiên: Số càng cao, câu thoại này càng dễ được chọn.")]
        public int Priority = 0;

        [Tooltip("Đánh dấu nếu câu thoại này có thể được lặp lại nhiều lần.")]
        public bool IsRepeatable = false;

        /// <summary>
        /// Gets the localized dialogue text.
        /// If DialogueTextKey is set, uses LocalizationManager.
        /// Otherwise, falls back to legacy DialogueText field.
        /// </summary>
        public string GetText()
        {
            // New way: Use localization key
            if (!string.IsNullOrEmpty(DialogueTextKey))
            {
                return LocalizationManager.Instance.GetText(DialogueTextKey);
            }

            // Legacy way: Use direct text (for old SOs)
            return DialogueText;
        }
    }
}
