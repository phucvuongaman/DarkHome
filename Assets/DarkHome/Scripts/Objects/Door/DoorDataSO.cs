using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject data cho Door objects.
    /// Chứa thông tin về scene transition và spawn points.
    /// </summary>
    [CreateAssetMenu(fileName = "DOOR_", menuName = "SO/Objects/DoorSO")]
    public class DoorDataSO : ObjectDataSO
    {
        [Header("=== DOOR SPECIFIC ===")]
        [Tooltip("Tên scene đích khi đi qua cửa này")]
        public string nextSceneName;

        [Tooltip("ID của spawn point ở scene mới (nơi player xuất hiện)")]
        public string targetSpawnID;

        [TextArea(2, 4)]
        [Tooltip("Mô tả cho UI (hiển thị khi hover hoặc interact)")]
        public string description;
    }
}
