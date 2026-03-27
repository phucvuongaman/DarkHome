using UnityEngine;

namespace DarkHome
{
    public class PlayerMoveState : BaseState<PlayerStateMachine.EPlayerState>
    {
        private float _elapsedTime;

        protected PlayerContext _context;
        public PlayerMoveState(PlayerContext context, PlayerStateMachine.EPlayerState statekey)
        : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            _elapsedTime = 0f;
        }

        public override void UpdateState()
        {
            _context.Interactor.OnFocus();
        }

        public override void ExitState() { }


        public override PlayerStateMachine.EPlayerState GetNextState()
        {
            // Đọc input trực tiếp từ Context
            if (_context.MoveInput.magnitude < 0.1f)
            {
                return PlayerStateMachine.EPlayerState.Idle;
            }
            return StateKey;
        }

    }
}