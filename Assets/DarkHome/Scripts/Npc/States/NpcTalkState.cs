using UnityEngine;

namespace DarkHome
{
    public class NpcTalkState : BaseState<NpcStateMachine.ENpcStates>
    {
        private Transform _playerTransform;
        private bool _isConversationFinished = false;

        protected NpcContext _context;

        public NpcTalkState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;
        }

        public override void EnterState()
        {
            _isConversationFinished = false;

            // Tìm Player
            if (_context.ScannerTarget != null && _context.ScannerTarget.Target != null)
                _playerTransform = _context.ScannerTarget.Target.transform;
            else
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p) _playerTransform = p.transform;
            }

            // Setup Animator
            _context.Animator.SetBool("IsTalking", true);

            // Dừng di chuyển ngay lập tức (Dùng hàm mới đã nâng cấp)
            _context.NpcMovement.StopMove();

            // Tắt tự xoay của Agent để ta tự điều khiển xoay
            if (_context.Agent != null) _context.Agent.updateRotation = false;

            EventManager.AddObserver(GameEvents.DiaLog.EndDialogue, OnDialogueEnded);
        }

        public override void ExitState()
        {
            _context.Animator.SetBool("IsTalking", false);

            // Đảm bảo khi thoát ra thì Animation Speed về 0
            _context.NpcMovement.StopMove();

            // Trả lại quyền tự xoay cho Agent (để MoveState dùng)
            if (_context.Agent != null) _context.Agent.updateRotation = true;

            EventManager.RemoveListener(GameEvents.DiaLog.EndDialogue, OnDialogueEnded);
        }

        public override void UpdateState()
        {
            // Luôn gọi StopMove mỗi frame để ép Animation Speed giảm dần về 0
            _context.NpcMovement.StopMove();

            // HeadLook (nếu có)
            _context.HeadLook?.CheckingTarget();

            if (_playerTransform == null) return;

            // --- GỌI HÀM XOAY MỚI ---
            // Tốc độ 5f: Xoay mượt mà, tự nhiên (giống IdleState)
            _context.NpcMovement.RotateTowards(_playerTransform.position, 5f);
        }

        private void OnDialogueEnded() => _isConversationFinished = true;

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            return _isConversationFinished ? NpcStateMachine.ENpcStates.Idle : StateKey;
        }
    }
}