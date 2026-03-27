// Deleted
// Đọc ghi dữ liệu từ so 
// Dùng cho chuyển scene

using UnityEngine;

namespace DarkHome
{
    public class PlayerSaveAdapter : MonoBehaviour
    {
        [SerializeField] private PlayerDataSO _playerData;
        [SerializeField] private PlayerStats _stats; // ví dụ class PlayerStats { float Health, Sanity }

        void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        // void Start()
        // {
        //     // Khi spawn Player mới => Apply dữ liệu từ SO
        //     ApplyFromData();
        // }

        public void SaveToData()
        {
            if (_playerData == null) return;
            _playerData.Health = _stats.Health;
            _playerData.Sanity = _stats.Sanity;
        }

        public void ApplyFromData()
        {
            if (_playerData == null) return;
            _stats.Health = _playerData.Health;
            _stats.Sanity = _playerData.Sanity;
        }
    }


}