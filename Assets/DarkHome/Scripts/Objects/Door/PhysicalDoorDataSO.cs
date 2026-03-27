using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject data cho PhysicalDoor objects (in-scene doors).
    /// Kế thừa ObjectDataSO → Không cần thêm fields!
    /// RequiredFlags, onInteractTriggers đã có sẵn trong base class.
    /// </summary>
    [CreateAssetMenu(fileName = "PHYSICALDOOR_", menuName = "SO/Objects/PhysicalDoorSO")]
    public class PhysicalDoorDataSO : ObjectDataSO
    {
        // NO ADDITIONAL FIELDS NEEDED!
        // PhysicalDoor chỉ cần:
        // - objectID (base class)
        // - localizationKey (base class)
        // - requiredFlags (base class) → để lock/unlock
        // - onInteractTriggers (base class) → fire events when interact

        // HingeJoint setup thì làm trực tiếp trong Unity Inspector
        // AudioClips cũng vậy
    }
}
