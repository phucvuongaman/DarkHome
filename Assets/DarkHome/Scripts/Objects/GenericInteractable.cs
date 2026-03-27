using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Concrete implementation of BaseObject for generic interactable objects.
    /// Use for: SOFA, TV, SHELF, FRIDGE, CALENDAR, etc.
    /// These are objects that can be examined/used but have no special behavior.
    /// </summary>
    [RequireComponent(typeof(OutLineController))]
    public class GenericInteractable : BaseObject
    {
        public override InteractableType InteractType => InteractableType.GenericObject;

        protected OutLineController outLine;

        protected override void Awake()
        {
            base.Awake();
            outLine = GetComponent<OutLineController>();
        }

        public override void OnFocus()
        {
            outLine?.EnableOutline();
        }

        public override void OnLoseFocus()
        {
            outLine?.DisableOutline();
        }

        public override void OnInteractPress(Interactor interactor)
        {
            // Check if object has examine dialogue (player internal thought)
            if (_objectData is IHasExamineDialogue examinable)
            {
                string nodeId = examinable.GetExamineDialogueKey();
                if (!string.IsNullOrEmpty(nodeId))
                {
                    // Trigger player thought dialogue with NodeID directly!
                    EventManager.Notify(GameEvents.DiaLog.StartDialogueWithIdNode, nodeId);
                }
            }

            // Grant flags & trigger events (original behavior from BaseObject)
            base.OnInteractPress(interactor);
        }

        // No additional logic needed - all behavior inherited from BaseObject:
        // - requiredFlags/hidingFlags check
        // - onInteractTriggers fire
        // - Layer switching (Interactable/Inactive)
    }
}
