using UnityEngine;

namespace DarkHome
{
    public class NpcMoveState : BaseState<NpcStateMachine.ENpcStates>
    {
        protected NpcContext _context;

        public NpcMoveState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            if (_context.NpcMovement != null)
            {
                // Gọi hàm này để NpcMovement tự lo liệu việc mở khóa Agent
                _context.NpcMovement.MoveToTarget(_context.StateMachine.MoveTargetPosition);
            }
            else if (_context.Agent != null)
            {
                // Fallback phòng khi chưa gắn NpcMovement
                _context.Agent.isStopped = false;
                _context.Agent.updateRotation = true;
                _context.Agent.SetDestination(_context.StateMachine.MoveTargetPosition);
            }
        }

        public override void ExitState()
        {

            if (_context.NpcMovement != null)
            {
                _context.NpcMovement.StopMove();
            }
            else
            {
                _context.Animator.SetFloat("MoveSpeed", 0f);
                if (_context.Agent != null) _context.Agent.isStopped = true;
            }
        }

        public override void UpdateState()
        {
            // Cập nhật Animation Đi bộ (Lấy tốc độ từ Inspector của NpcMovement)
            _context.NpcMovement.AnimatorWalk();
        }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            //  PHẦN QUAN TRỌNG
            // Nếu là Enemy, đang đi tuần mà thấy Player -> Chuyển sang Rage ngay
            if (_context.ScannerTarget != null && _context.ScannerTarget.CheckingTarget())
            {
                if (_context is EnemyContext)
                {
                    return NpcStateMachine.ENpcStates.Rage;
                }
            }

            // LOGIC KIỂM TRA ĐẾN ĐÍCH
            // Enemy: Wait → GuardZone → tiếp tục patrol
            // NPC thường: Idle
            bool isEnemy = _context is EnemyContext;
            var onArriveState = isEnemy
                ? NpcStateMachine.ENpcStates.Wait
                : NpcStateMachine.ENpcStates.Idle;

            if (_context.NpcMovement != null && _context.NpcMovement.HasReachedDestination)
            {
                return onArriveState;
            }

            // Fallback kiểm tra thủ công (nếu NpcMovement chưa chuẩn)
            if (_context.Agent != null && !_context.Agent.pathPending)
            {
                if (_context.Agent.remainingDistance <= _context.Agent.stoppingDistance)
                {
                    if (!_context.Agent.hasPath || _context.Agent.velocity.sqrMagnitude == 0f)
                    {
                        return onArriveState;
                    }
                }
            }

            return StateKey;
        }
    }
}