using UnityEngine;

namespace DarkHome
{
    public class NpcRageState : BaseState<NpcStateMachine.ENpcStates>
    {
        // Nó vẫn nhận NpcContext chung...
        protected NpcContext _context;
        // ...nhưng nó cần một EnemyContext cụ thể để làm việc
        protected EnemyContext _enemyContext; // Ép kiểu context chung sang context chuyên biệt

        // Các component chuyên biệt
        private ScannerTarget _scanner;
        private EnemyMove _enemyMove;
        private EnemyRage _enemyRage;


        private float _lostSightTime;
        private const float CHASE_DURATION = 3f;

        // Constructor vẫn nhận NpcContext chung
        public NpcRageState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _context = context;

            // --- ĐÂY LÀ MẤU CHỐT ---
            // Tự "ép kiểu" (cast) context chung sang context chuyên biệt
            _enemyContext = context as EnemyContext;
        }

        public override void EnterState()
        {
            // Debug.Log("EnterState Rage");
            if (_enemyContext == null)
            {
                Debug.LogError("NpcRageState được gán cho một NPC không phải là Enemy!");
                return;
            }

            _scanner = _enemyContext.ScannerTarget;
            _enemyMove = _enemyContext.EnemyMove;
            _enemyRage = _enemyContext.EnemyRage;

            // 1. Hét lên (SFX)
            _enemyContext.EnemyAudio.PlayScream();
            // _enemyContext.EnemyAudio.PlayHeartBeat();


            // 2. Bật nhạc nền (Music - Fade In)
            _enemyContext.EnemyAudio.RequestChaseMusic();

        }

        public override void ExitState()
        {
            _enemyMove.StopChasing();  // Đảm bảo dừng đuổi khi thoát state

            // KHÔNG TẮT NHẠC Ở ĐÂY!
            // Vì nếu tắt ở đây, khi nó chuyển sang Confuse (vẫn đang tìm) mà nhạc tắt thì tụt mood.
            // Ta sẽ tắt khi nó thực sự bỏ cuộc.
        }

        // Trong file: NpcRageState.cs
        public override void UpdateState()
        {
            if (_enemyContext == null) return;

            // Ra lệnh cho "cơ bắp" và "vũ khí"
            _context.NpcMovement.AnimatorRun();
            _enemyMove.ChasingToTarget();

            // Nếu vẫn còn đang nhìn thấy Player...
            if (_scanner.CheckingTarget())
            {
                // ...thì liên tục cập nhật vị trí cuối cùng của anh ta.
                _context.StateMachine.LastKnownPlayerPosition = _scanner.Target.position;
            }

            if (_enemyRage.ScanAttackRange(out Transform target))
            {
                _enemyRage.StartAttack();
            }
        }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            if (_enemyContext == null) return StateKey; // Trả về state hiện tại
            // Logic ra quyết định (bộ não)
            if (_scanner.CheckingTarget())
            {
                _lostSightTime = CHASE_DURATION; // Thấy Player, reset đếm
            }
            else
            {
                _lostSightTime -= Time.deltaTime;
                if (_lostSightTime <= 0)
                {
                    return NpcStateMachine.ENpcStates.Confuse; // Không thấy Player -> Chuyển sang bối rối
                }
            }
            return StateKey; // Vẫn đang đuổi
        }
    }
}