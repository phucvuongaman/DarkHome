using UnityEngine;
using System;

namespace DarkHome
{
    public class Npc : BaseInteractable
    {
        [SerializeField]
        public string Description;

        private NpcContext _npcContext;


        private void Awake()
        {
            _npcContext = GetComponent<NpcContext>();
        }

        #region IInteractable Interface Implementation
        public override InteractableType InteractType { get; set; } = InteractableType.NPC;
        public override void OnInteractPress(Interactor interactor)
        {
            if (_npcContext.NpcMovement.IsMoving) return;

            EventManager.Notify(GameEvents.DiaLog.StartDialogueWithIdSpeaker, Id);
            _npcContext.StateMachine.TransitionToState(NpcStateMachine.ENpcStates.Talk);
        }
        #endregion


    }
}