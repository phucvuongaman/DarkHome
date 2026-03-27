using UnityEngine;

namespace DarkHome
{
    public class NpcIdleState : BaseState<NpcStateMachine.ENpcStates>
    {
        protected NpcContext _context;

        public NpcIdleState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            // Reset Animation về đứng yên
            // _context.Animator.SetBool("IsWalking", false);
            _context.Animator.SetFloat("MoveSpeed", 0f);

            // Dừng hẳn Agent
            _context.NpcMovement.StopMove();
        }

        public override void ExitState()
        {
            // Không cần làm gì đặc biệt
        }

        public override void UpdateState()
        {
            // Luôn đảm bảo đứng yên (giảm tốc độ animation)
            _context.NpcMovement.StopMove();

            // HeadLook (Nếu có) - Để đầu tự liếc nhìn Player nếu ở gần
            _context.HeadLook?.CheckingTarget();

            // LOGIC XOAY VỀ HƯỚNG ANCHOR
            if (_context.CurrentAnchor != null && _context.CurrentAnchor.SnapRotation)
            {
                // MẸO: Tạo một điểm mục tiêu ảo nằm phía trước Anchor
                // (Vì RotateTowards cần một điểm để nhìn vào)
                Vector3 targetLookPos = _context.transform.position + _context.CurrentAnchor.transform.forward * 5f;

                // Gọi hàm xoay từ NpcMovement
                // Tốc độ 5f: Xoay từ từ, chậm rãi hơn lúc nói chuyện (Idle mà)
                _context.NpcMovement.RotateTowards(targetLookPos, 5f);
            }
        }
        public override NpcStateMachine.ENpcStates GetNextState()
        {
            // "Não" của Idle là liên tục scan
            if (_context.ScannerTarget != null && _context.ScannerTarget.CheckingTarget())
            {
                if (_context is EnemyContext)
                {
                    return NpcStateMachine.ENpcStates.Rage;
                }
            }
            return StateKey;
        }
    }
}