using UnityEngine;

namespace DarkHome
{
    public class PlayerTalkState : BaseState<PlayerStateMachine.EPlayerState>
    {
        protected PlayerContext _context;
        public PlayerTalkState(PlayerContext context, PlayerStateMachine.EPlayerState statekey)
        : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            EventManager.Notify(GameEvents.Camera.EnableCamRotate, false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Freeze Y rotation và clear angular velocity để tránh physics drift khi đứng sát NPC
            _context.Rb.angularVelocity = Vector3.zero;
            _context.Rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        }
        public override void UpdateState() { }

        public override void ExitState()
        {
            EventManager.Notify(GameEvents.Camera.EnableCamRotate, true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Restore Y rotation để player có thể xoay bình thường khi đi lại
            _context.Rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        }

        public override PlayerStateMachine.EPlayerState GetNextState()
        {
            return StateKey;
        }
    }
}