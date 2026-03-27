using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace DarkHome
{
    // HƯỚNG DẪN CHỌN TYPE:
    // - Dialogue: "Data" là ID của Node thoại cần nhảy tới.
    // - Quest: "Data" là ID của Quest cần kích hoạt.
    // - SceneTransition: "Data" là Tên Scene, "TargetObject" là SpawnPoint ở scene mới.
    // - FlagOnly: Chỉ dùng để bật/tắt cờ (GrantedFlags).

    [Serializable]
    public enum ETriggerType
    {
        Dialogue,       // Data: ID Node
        Cutscene,       // Data: ID Cutscene
        Quest,          // Data: ID Quest

        // --- Nhóm Tương Tác Object (Dùng TargetObject) ---
        ObjectToggle,   // Bật/Tắt
        Spawn,         // Hiện
        Despawn,       // Ẩn
        Animation,    // Data: Trigger Name (TargetObject: Animator)

        // --- Nhóm Âm Thanh (Dùng TargetObject chứa AudioSource/AudioZone) ---
        Sound,       // SFX 1 lần
        MusicChange,   // Đổi nhạc nền
        AmbienceChange, // Đổi môi trường

        // --- Nhóm Di Chuyển ---
        Teleport,     // TargetObject: Điểm đến (Data: Trống)
        SceneTransition,// Data: "SceneName:SpawnID"

        FlagOnly,    // Chỉ nhận thưởng
        SaveGame,   // Lưu game

        StoryEvent,   // Data: Tên sự kiện (VD: "Event_Dinner", "9AM_Bell")

    }


    [CreateAssetMenu(fileName = "EventSO", menuName = "SO/Event/EventTrigger")]
    public class EventTriggerDataSO : ScriptableObject
    {
        public List<EventTrigger> triggers;
    }


    // Đoạn này cần logic trong editor
    [Serializable]
    public class EventTrigger
    {
        [Tooltip("ID định danh cho sự kiện này (VD: 'EVT_Talk_Teacher'). Dùng ID này để gọi từ script.")]
        public string Id;

        [Tooltip("Điều kiện CẦN: Sự kiện này chỉ chạy nếu Player CÓ đủ các cờ này.")]
        public List<FlagData> RequiredFlags;

        [Tooltip("Phần thưởng: Sau khi chạy xong, Player sẽ nhận được các cờ này.")]
        public List<FlagData> GrantedFlags;

        [Header("Action Settings")]
        [Tooltip("Loại hành động sẽ thực hiện.")]
        public ETriggerType Type;

        [Tooltip("Đối tượng bị tác động:\n" +
                 "- Teleport: Kéo điểm đến (Empty GO) vào đây.\n" +
                 "- Sound/Music: Kéo object có AudioSource vào đây.\n" +
                 "- Anim/Object: Kéo object cần điều khiển vào đây.")]
        public GameObject TargetObject;

        [Header("Data Configuration")]
        [Tooltip(
            "Hướng dẫn điền Data (Nếu không liệt kê -> ĐỂ TRỐNG):\n\n" +
            "• Dialogue: ID Node thoại (VD: 'Node_Start')\n" +
            "• Quest: ID Quest (VD: 'QUEST_01')\n" +
            "• SceneTransition: 'TênScene:SpawnID' (VD: 'Level1:Spawn_A')\n" +
            "• Animation: Tên Parameter Trigger (VD: 'Open')\n" +
            "• Cutscene: Tên/ID của Timeline (nếu có)"
        )]
        [TextArea(3, 10)]
        public string Data;
    }
}