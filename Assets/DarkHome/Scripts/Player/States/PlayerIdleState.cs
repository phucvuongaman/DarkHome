using UnityEngine;

namespace DarkHome
{
    public class PlayerIdleState : BaseState<PlayerStateMachine.EPlayerState>
    {
        protected PlayerContext _context;
        public PlayerIdleState(PlayerContext context, PlayerStateMachine.EPlayerState statekey)
        : base(statekey)
        {
            _context = context;
        }
        public override void EnterState() { }
        public override void UpdateState()
        {
            _context.Interactor.OnFocus();
        }
        public override void ExitState() { }

        public override PlayerStateMachine.EPlayerState GetNextState()
        {

            if (_context.MoveInput.magnitude > 0.1f)
            {
                return PlayerStateMachine.EPlayerState.Move;
            }
            return StateKey;
        }

    }
}