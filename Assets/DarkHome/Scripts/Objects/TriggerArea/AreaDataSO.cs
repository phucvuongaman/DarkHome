using UnityEngine;
using System.Collections.Generic;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject data cho TriggerArea objects.
    /// Chứa thông tin về invisible trigger zones (cutscenes, ambiance).
    /// </summary>
    [CreateAssetMenu(fileName = "AREA_", menuName = "SO/Objects/AreaSO")]
    public class AreaDataSO : ObjectDataSO
    {
        [Header("=== TRIGGER AREA SPECIFIC ===")]
        [Tooltip("Event Triggers kích hoạt khi Player vào zone")]
        public List<FlagData> onEnterTriggers = new List<FlagData>();

        [Tooltip("Event Triggers kích hoạt khi Player rời zone")]
        public List<FlagData> onExitTriggers = new List<FlagData>();

        [Tooltip("Trigger chỉ kích hoạt 1 lần duy nhất? (Sau đó tự disabled)")]
        public bool triggerOnce = true;
    }
}
