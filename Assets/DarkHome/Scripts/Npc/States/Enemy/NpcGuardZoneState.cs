using UnityEngine;
using UnityEngine.AI;

namespace DarkHome
{
    public class NpcGuardZoneState : BaseState<NpcStateMachine.ENpcStates>
    {
        private EnemyContext _enemyContext;
        private NpcStateMachine _stateMachine;
        private const int MAX_LOCAL_PATROLS = 3;

        public NpcGuardZoneState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _enemyContext = context as EnemyContext;
            _stateMachine = context.StateMachine;
        }

        public override void EnterState()
        {
            if (_enemyContext.localPatrolsDone >= MAX_LOCAL_PATROLS)
            {
                return;
            }

            // Tìm một điểm ngẫu nhiên HỢP LỆ trong phòng
            Vector3 randomPoint = GetRandomNavMeshPointInZone();

            _stateMachine.SetMoveTarget(randomPoint);

            _enemyContext.localPatrolsDone++;
        }

        public override void ExitState() { }

        public override void UpdateState() { }

        public override NpcStateMachine.ENpcStates GetNextState()
        {
            if (_enemyContext.localPatrolsDone >= MAX_LOCAL_PATROLS)
            {
                return NpcStateMachine.ENpcStates.ChoosePatrolZone;
            }

            // Luôn luôn gọi lính "Đi Bộ" (Move) để đi tuần tra.
            return NpcStateMachine.ENpcStates.Move;
        }

        private Vector3 GetRandomNavMeshPointInZone()
        {
            // Cố gắng tìm một điểm hợp lệ trong 10 lần
            for (int i = 0; i < 10; i++)
            {
                // Tạo một điểm ngẫu nhiên "mù" như cũ
                Vector2 randomInCircle = Random.insideUnitCircle * _enemyContext.currentZone.patrolRadius;
                Vector3 randomPoint = _enemyContext.currentZone.centerPoint.position + new Vector3(randomInCircle.x, 0, randomInCircle.y);

                NavMeshHit hit;
                // Dùng SamplePosition để tìm điểm hợp lệ gần đó trong bán kính 1.0m
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    // Nếu tìm thấy, trả về điểm hợp lệ ngay lập tức
                    return hit.position;
                }
            }

            // Nếu sau 10 lần vẫn không tìm được, trả về vị trí trung tâm cho an toàn
            Debug.LogWarning($"Không thể tìm thấy điểm NavMesh hợp lệ trong khu vực {_enemyContext.currentZone.zoneName}. Quay về trung tâm.");
            return _enemyContext.currentZone.centerPoint.position;
        }
    }
}
