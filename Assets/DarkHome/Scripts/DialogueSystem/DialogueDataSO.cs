using System.Collections.Generic;
using UnityEngine;


namespace DarkHome
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "DialogueDataSO", menuName = "SO/Dialogue/Dialogue Data")]
    public class DialogueDataSO : ScriptableObject
    {
        // HƯỚNG DẪN SỬ DỤNG:
        // 1. NpcId: Phải khớp CHÍNH XÁC với ID trong script "Npc.cs" trên GameObject NPC.
        //    Ví dụ: Nếu NPC trong scene có ID là "Teacher_01", thì ở đây cũng phải điền "Teacher_01".
        [Tooltip("ID của người nói (NPC). Để trống nếu đây là thoại môi trường/tự sự.")]
        public string NpcId;

        // 2. Nodes: Danh sách các câu thoại.
        //    - IsStartNode: Tích vào đây nếu muốn câu này nói ĐẦU TIÊN khi gặp nhau.
        //    - NodeId: Đặt tên dễ nhớ (VD: "Start", "Quest_Offer", "Quest_Done").
        //    - NextId: ID của câu tiếp theo sẽ nhảy tới (nếu không có lựa chọn).
        //    - RequiredFlags: Các cờ cần có để nói câu này (VD: Phải có cờ "Chapter1_Start").
        [Tooltip("Danh sách tất cả các câu thoại thuộc về file này.")]
        public List<DialogueNode> Nodes;
    }
}