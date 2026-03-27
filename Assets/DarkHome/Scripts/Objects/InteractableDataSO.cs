using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// ScriptableObject for generic interactable objects (furniture, electronics, appliances).
    /// These are objects the player can interact with but NOT collect.
    /// Examples: SOFA, TV, FRIDGE, CALENDAR, MIRROR
    /// </summary>
    [CreateAssetMenu(fileName = "INTERACTABLE_", menuName = "SO/Objects/InteractableObject")]
    public class InteractableDataSO : ObjectDataSO, IHasExamineDialogue
    {
        // Inherits all fields from ObjectDataSO:
        // - objectID
        // - requiredFlags
        // - hidingFlags
        // - onInteractTriggers
        // - localizationKey

        [Header("=== EXAMINE DIALOGUE ===")]
        [Tooltip("TextKey của dialogue suy nghĩ khi player interact (VD: DIALOGUE_PLAYER_EXAMINE_TV_L1). Leave empty nếu không cần dialogue.")]
        public string examineDialogueKey;

        public string GetExamineDialogueKey()
        {
            return examineDialogueKey;
        }
    }
}
