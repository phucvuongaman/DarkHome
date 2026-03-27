using UnityEngine;
using System.Collections.Generic;
using System;

namespace DarkHome
{
    [RequireComponent(typeof(NpcContext))]
    public class NpcScheduler : MonoBehaviour
    {
        // Nhét struct vào trong này cho gọn namespace
        // Dùng 'struct' thay vì 'class' vì nó chỉ chứa data đơn giản -> Nhẹ hơn
        [Serializable]
        public struct ScheduleData
        {
            public string EventID; // Tên sự kiện (VD: "Event_Dinner")
            public NpcAnchor TargetAnchor; // Điểm đến
        }

        [Header("Lịch Trình NPC")]
        // Inspector sẽ hiện ra cái List này để ông điền
        [SerializeField] private List<ScheduleData> _schedules;

        private NpcContext _npcContext;

        private void Awake()
        {
            _npcContext = GetComponent<NpcContext>();
        }

        private void OnEnable()
        {
            EventManager.AddObserver<string>(GameEvents.Story.OnStoryEvent, OnStoryEventTriggered);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Story.OnStoryEvent, OnStoryEventTriggered);
        }

        private void OnStoryEventTriggered(string eventId)
        {
            // Dò tìm trong List 
            // Debug.Log($" Scheduler {name} nghe thấy: [{eventId}]");

            foreach (var entry in _schedules)
            {
                // Debug thêm cái này để soi từng dòng trong list
                Debug.Log($"   - So sánh với: [{entry.EventID}]");
                // So sánh chuỗi (String Comparison)
                if (entry.EventID.Equals(eventId))
                {
                    ChangeLocation(entry.TargetAnchor);
                    return;
                }
            }
        }

        private void ChangeLocation(NpcAnchor newAnchor)
        {
            if (newAnchor == null) return;
            // Debug.Log($" {name}: Đổi địa điểm sang {newAnchor.name}");

            // Cập nhật bộ nhớ và ép di chuyển
            _npcContext.CurrentAnchor = newAnchor;
            _npcContext.StateMachine.SetMoveTarget(newAnchor.transform.position);
            _npcContext.StateMachine.TransitionToState(NpcStateMachine.ENpcStates.Move);
        }
    }
}