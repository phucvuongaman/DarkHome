using UnityEngine;

namespace DarkHome
{
    public class NpcConfuseState : BaseState<NpcStateMachine.ENpcStates>
    {
        // === CÁC BIẾN QUẢN LÝ ===
        private float _confuseTimer = 0f;
        private const float CONFUSE_DURATION = 4f; // Thời gian đứng nhìn xung quanh
        private bool _hasArrivedAtLastPosition = false; // "Công tắc"

        protected NpcContext _context;
        private ScannerTarget _scanner;

        public NpcConfuseState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
            _scanner = _context.ScannerTarget;
        }

        public override void EnterState()
        {
            // Reset mọi thứ khi bắt đầu trạng thái
            _confuseTimer = 0f;
            _hasArrivedAtLastPosition = false;

            // GIAI ĐOẠN 1: Ra lệnh di chuyển đến vị trí cuối cùng nhìn thấy Player
            // (Chúng ta cần đảm bảo RageState đã lưu vị trí này)
            _context.NpcMovement.MoveToTarget(_context.StateMachine.LastKnownPlayerPosition);

            // Tắt nhạc nền (Fade Out) khi mất dấu
            // _context.GetComponent<EnemyAudio>()?.StopChaseMusic();
            // Lưu ý: NpcContext chưa có property EnemyAudio public gọn gàng như EnemyContext, 
            // nên dùng GetComponent hoặc ép kiểu nếu lười sửa Context.
            // Tốt nhất là ép kiểu:
            if (_context is EnemyContext enemyCtx)
            {
                enemyCtx.EnemyAudio.StopChaseMusicRequest();
            }
        }

        public override void ExitState()
        {
            _context.Animator.SetBool("IsConfuse", false);
        }

        public override void UpdateState()
        {
            // === KIỂM TRA "CÔNG TẮC" ===
            // Nếu chưa đến nơi (công tắc đang TẮT)
            if (!_hasArrivedAtLastPosition)
            {
                _context.NpcMovement.AnimatorWalk(); // Cứ đi bộ tới đó

                // Kiểm tra xem đã đến nơi chưa
                if (_context.NpcMovement.IsIdle)
                {
                    _hasArrivedAtLastPosition = true; // BẬT công tắc
                    _context.Animator.SetBool("IsConfuse", true); // Bắt đầu animation bối rối
                }
            }
            // Nếu đã đến nơi (công tắc đang BẬT)
            else
            {
                // GIAI ĐOẠN 2: Bắt đầu bấm giờ
                _confuseTimer += Time.deltaTime;
                _context.HeadLook?.CheckingTarget(); // Nhìn xung quanh
                _context.NpcMovement.StopMove();
            }
        }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            // Ưu tiên 1: Thấy lại Player -> Nổi giận ngay!
            if (_scanner != null && _scanner.CheckingTarget())
            {
                if (_context is EnemyContext) return NpcStateMachine.ENpcStates.Rage;
            }

            // Ưu tiên 2: Khi đã đến nơi VÀ đã hết giờ bối rối
            if (_hasArrivedAtLastPosition && _confuseTimer >= CONFUSE_DURATION)
            {
                if (_context is EnemyContext)
                {
                    // Quay lại nhiệm vụ tuần tra
                    return NpcStateMachine.ENpcStates.ChoosePatrolZone;
                }
                else
                {
                    return NpcStateMachine.ENpcStates.Idle;
                }
            }

            return StateKey; // Vẫn đang trong trạng thái này (đang đi hoặc đang chờ)
        }
    }
}