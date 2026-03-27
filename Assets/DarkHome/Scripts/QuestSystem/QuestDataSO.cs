using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [Serializable]
    [CreateAssetMenu(fileName = "QuestDataSO", menuName = "SO/Quest/Quest Data")]
    public class QuestDataSO : ScriptableObject
    {
        // HƯỚNG DẪN TẠO QUEST:
        // - Id: Mã nhiệm vụ (VD: 'QUEST_01').
        // - Objectives: Các mục tiêu con.
        //   + targetID: ID của Item cần nhặt hoặc NPC cần nói chuyện.
        //   + description: Dòng chữ hiện trên bảng nhiệm vụ (VD: "Tìm chìa khóa").

        [Tooltip("ID của Chapter mà bộ Quest này thuộc về (VD: 'Chapter1').")]
        public string chapterId;


        [Tooltip("Danh sách các nhiệm vụ.")]
        public List<Quest> quests;


    }
}