using UnityEngine;
using System.Collections.Generic;

namespace DarkHome
{
    public class ActiveByDay : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Object này sẽ xuất hiện vào những ngày nào? (Ví dụ: 1, 3)")]
        [SerializeField] private List<int> _activeOnDays;

        [Tooltip("Nếu tích vào đây: Object sẽ hoạt động NGƯỢC LẠI (Ẩn vào những ngày trên)")]
        [SerializeField] private bool _reverseLogic = false;

        // Track subscription để tránh subscribe nhiều lần
        private bool _isSubscribed = false;

        private void OnEnable()
        {
            CheckVisibility();

            // Chỉ subscribe 1 lần duy nhất
            if (!_isSubscribed)
            {
                EventManager.AddObserver<int>(GameEvents.Day.OnDayChanged, OnDayChangedHandler);
                _isSubscribed = true;
            }
        }



        private void OnDestroy()
        {
            // Chỉ unsubscribe khi bị Destroy thật sự
            if (_isSubscribed)
            {
                EventManager.RemoveListener<int>(GameEvents.Day.OnDayChanged, OnDayChangedHandler);
                _isSubscribed = false;
            }
        }

        private void OnDayChangedHandler(int newDay)
        {
            // Gọi được ngay cả khi object đang inactive — đây là ý định thiết kế
            CheckVisibility();
        }

        public void CheckVisibility()
        {
            if (SaveLoadManager.Instance == null) return;

            int currentDay = SaveLoadManager.Instance.GetCurrentDay();
            bool isDayInList = _activeOnDays.Contains(currentDay);
            bool shouldActive = _reverseLogic ? !isDayInList : isDayInList;

            gameObject.SetActive(shouldActive);
        }
    }
}
