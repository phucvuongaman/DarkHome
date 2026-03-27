using UnityEngine;

namespace DarkHome
{
    public class NpcWaitState : BaseState<NpcStateMachine.ENpcStates>
    {
        private NpcContext _context;

        private float _waitTimer;
        private float _waitTime;

        public NpcWaitState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            // Mỗi lần chờ sẽ có thời gian ngẫu nhiên
            _waitTime = Random.Range(2f, 5f);
            _waitTimer = 0f;

            // Bạn có thể bật một animation "nhìn ngó" ở đây
            // _context.Animator.SetBool("IsGuarding", true);
        }

        public override void ExitState()
        {
            // Tắt animation "nhìn ngó"
            // _context.Animator.SetBool("IsGuarding", false);
        }

        public override void UpdateState()
        {
            _context.NpcMovement.StopMove(); // Đảm bảo dừng di chuyển
            _waitTimer += Time.deltaTime;
            _context.HeadLook?.CheckingTarget(); // Vẫn cho phép nhìn xung quanh
        }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            // Nếu thấy Player, ưu tiên hàng đầu là RAGE!
            if (_context.ScannerTarget != null && _context.ScannerTarget.CheckingTarget())
            {
                return NpcStateMachine.ENpcStates.Rage;
            }

            // Hết giờ chờ, quay lại GuardZone để chọn điểm đi tiếp trong phòng
            if (_waitTimer >= _waitTime)
            {
                return NpcStateMachine.ENpcStates.GuardZone;
            }

            return StateKey; // Vẫn đang chờ
        }
    }
}