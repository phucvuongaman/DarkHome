using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{

    public enum EQuestObjectiveType
    {
        Collect, // Nhặt đồ (Sữa, Chìa khóa...)
        Talk, // Nói chuyện với NPC
        MoveTo, // Đi đến địa điểm (Văn phòng...)
        Kill, // Diệt địch (Nếu có)
        Interact // Tương tác đồ vật (Mở két, đọc giấy...)
    }
    /// <summary>
    /// Quest Objective - Individual task within a quest
    /// Uses localization keys for multi-language support
    /// </summary>
    [System.Serializable]
    public class QuestObjective
    {
        [Header("Cấu hình Mục tiêu")]
        [Tooltip("Loại mục tiêu: Nhặt, Nói chuyện, hay Đi đến...")]
        public EQuestObjectiveType Type;

        [Tooltip("Localization key for objective description (e.g., 'QUEST_C1_FIND_KEY_OBJ1')\nRuntime displays localized text")]
        public string DescriptionKey;

        // ========================================================================
        // HỆ THỐNG (FLAG SYSTEM) - Dùng cho Cốt truyện, Hội thoại
        // ========================================================================

        [Header("Logic Flag (New System)")]

        [Tooltip("ĐIỀU KIỆN HOÀN THÀNH:\n" +
                 "Danh sách các Flag cần có để mục tiêu này được tính là XONG.\n" +
                 "VD: Flag 'HAS_MILK' (Đã có sữa).")]
        public List<FlagData> CompletionFlags;

        [Tooltip("EVENT TRIGGERS KHI HOÀN THÀNH:\n" +
                 "Events được fire khi objective complete (e.g. EVENT_OBJ_COMPLETE, EVENT_CUTSCENE).\n" +
                 "Các events này sẽ set flags, trigger quests, etc.")]
        public List<FlagData> OnCompleteTriggers;

        [Tooltip("ĐIỀU KIỆN XUẤT HIỆN:\n" +
                 "Mục tiêu này sẽ BỊ ẨN cho đến khi người chơi có đủ các Flag này.\n" +
                 "Lưu ý: Biến này chỉ có tác dụng nếu Quest chọn chế độ hiển thị là 'Custom'.")]
        public List<FlagData> RequiredFlagsToAppear;

        [Tooltip("TÙY CHỈNH UI:\n" +
                 "True (Tích): Làm xong sẽ BIẾN MẤT khỏi bảng nhiệm vụ (Gọn gàng).\n" +
                 "False (Bỏ tích): Làm xong sẽ bị GẠCH NGANG (Kiểu check-list).")]
        public bool HideOnCompletion;

        // ========================================================================
        //  HỆ THỐNG (LEGACY) - Dùng cho Item, Số lượng
        // ========================================================================

        [Header("Logic Cũ (Legacy - Dùng cho Item)")]
        [Tooltip("ID của Item hoặc Trigger cần tương tác (VD: 'Item_MilkBox').")]
        public string TargetID;

        [Tooltip("Số lượng hiện tại đã thu thập được.")]
        public int CurrentAmount;

        [Tooltip("Số lượng yêu cầu (Mặc định là 1).")]
        public int RequiredAmount = 1;

        // ========================================================================
        // CẦU NỐI LOGIC (BRIDGE)
        // ========================================================================

        /// <summary>
        /// Đây là "Trọng Tài" quyết định mục tiêu đã xong hay chưa.
        /// Nó kiểm tra cả 2 hệ thống: Flag (Mới) và Số lượng (Cũ).
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                // ƯU TIÊN 1: Kiểm tra Flag (Hệ thống mới)
                // Nếu có cài đặt Flag hoàn thành, thì chỉ cần đủ Flag là xong.
                if (CompletionFlags != null && CompletionFlags.Count > 0)
                {
                    if (FlagManager.Instance.HasAllFlags(CompletionFlags)) return true;
                }

                // ƯU TIÊN 2: Kiểm tra Số lượng (Hệ thống cũ - Item)
                // Nếu không dùng Flag, hoặc chưa đủ Flag, thì kiểm tra xem đủ số lượng item chưa.
                if (!string.IsNullOrEmpty(TargetID))
                {
                    if (CurrentAmount >= RequiredAmount) return true;
                }

                return false; // Chưa xong gì cả
            }
        }
    }
}