using UnityEngine;
using System.Collections.Generic;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject data cho Bed objects.
    /// Chứa thông tin về sleep/loop mechanics và chapter restrictions.
    /// </summary>
    [CreateAssetMenu(fileName = "BED_", menuName = "SO/Objects/BedSO")]
    public class BedDataSO : ObjectDataSO
    {
        [Header("=== BED SPECIFIC ===")]
        [Tooltip("Danh sách Chapter IDs cho phép ngủ/loop (VD: Chapter1, Chapter2)")]
        public List<string> loopableChapterIDs = new List<string>();

        [Tooltip("ID của spawn point khi thức dậy sau khi ngủ")]
        public string wakeUpSpawnID = "WakeUpPoint";

        [Tooltip("Dialogue ID hiển thị khi Player cố ngủ ở chapter không cho phép")]
        public string cantSleepDialogueID;
    }
}
