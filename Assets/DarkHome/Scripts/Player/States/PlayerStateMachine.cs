using UnityEngine;

namespace DarkHome
{

    public class PlayerStateMachine : StateManager<PlayerStateMachine.EPlayerState>
    {
        public enum EPlayerState
        {
            Idle,
            Move,
            Talk,
        }

        PlayerContext _context;
        public EPlayerState CurrentStateKey => CurrentState.StateKey;
        void Awake()
        {
            _context = GetComponent<PlayerContext>();
            InitializeStates();
            CurrentState = States[EPlayerState.Idle];
        }

        private void InitializeStates()
        {
            States.Add(EPlayerState.Idle, new PlayerIdleState(_context, EPlayerState.Idle));
            States.Add(EPlayerState.Move, new PlayerMoveState(_context, EPlayerState.Move));
            States.Add(EPlayerState.Talk, new PlayerTalkState(_context, EPlayerState.Talk));
        }

        private void OnEnable()
        {
            EventManager.AddObserver<DialogueNode>(GameEvents.DiaLog.ReadDialogue, HandleDialogueStart);
            EventManager.AddObserver(GameEvents.DiaLog.EndDialogue, HandleDialogueEnd);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<DialogueNode>(GameEvents.DiaLog.ReadDialogue, HandleDialogueStart);
            EventManager.RemoveListener(GameEvents.DiaLog.EndDialogue, HandleDialogueEnd);
        }

        private void HandleDialogueStart(DialogueNode node)
        {
            TransitionToState(EPlayerState.Talk);
        }

        private void HandleDialogueEnd()
        {
            if (CurrentStateKey == EPlayerState.Talk)
            {
                TransitionToState(EPlayerState.Idle);
            }
        }

    }

}