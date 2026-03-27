using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    // [MỚI] Enum quy định cách hiển thị các mục tiêu
    public enum EQuestDisplayMode
    {
        Sequential, // TUẦN TỰ: Làm xong cái 1 mới hiện cái 2
        Parallel, // SONG SONG: Hiện tất cả cùng lúc (Kiểu đi chợ)
        Custom, // TÙY CHỈNH: Dùng flag riêng để ẩn hiện (Logic cũ)
    }

    /// <summary>
    /// Quest data structure - Stores quest logic and localization keys
    /// Actual display text is loaded from CSV via LocalizationManager
    /// </summary>
    [System.Serializable]
    public class Quest
    {
        [Header("Quest Identification")]
        [Tooltip("Unique quest ID (e.g., 'QUEST_C1_FIND_KEY')")]
        public string Id;

        [Header("Localization Keys")]
        [Tooltip("Localization key for quest name (e.g., 'QUEST_C1_FIND_KEY_name')\nRuntime loads text from EN.csv/VN.csv")]
        public string QuestNameKey;

        [Tooltip("Localization key for quest description (e.g., 'QUEST_C1_FIND_KEY_desc')")]
        public string DescriptionKey;

        public EQuestType Type;
        public EQuestStatus Status;

        [Tooltip("Nếu tích vào, Quest này sẽ chạy ngầm và không hiện lên bảng nhiệm vụ.")]
        public bool IsHidden = false;

        [Header("Điều Kiện Nhận Quest")]
        public List<FlagData> RequiredFlags;

        [Header("Event Triggers Khi Hoàn Thành Quest")]
        [Tooltip("Events được fire khi quest complete (e.g. EVENT_QUEST_COMPLETE, EVENT_CUTSCENE_ENDING)")]
        public List<FlagData> OnCompleteTriggers;


        [Header("Cấu Hình Hiển Thị (MỚI)")]
        [Tooltip("Sequential: Hiện từng cái một. Parallel: Hiện hết.")]
        public EQuestDisplayMode DisplayMode = EQuestDisplayMode.Sequential;

        [Header("Danh Sách Mục Tiêu")]
        public List<QuestObjective> Objectives;
    }

    // Enum loại Quest
    public enum EQuestType
    {
        Main,
        Side
    }

    // Enum trạng thái
    public enum EQuestStatus
    {
        Inactive,
        Active,
        Completed,
        Failed,
        Skipped
    }
}