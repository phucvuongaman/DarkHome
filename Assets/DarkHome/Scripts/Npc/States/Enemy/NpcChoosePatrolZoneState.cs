using UnityEngine;
using System.Collections.Generic;

namespace DarkHome
{
    public class NpcChoosePatrolZoneState : BaseState<NpcStateMachine.ENpcStates>
    {
        private EnemyContext _enemyContext;

        public NpcChoosePatrolZoneState(NpcContext context, NpcStateMachine.ENpcStates statekey) : base(statekey)
        {
            _enemyContext = context as EnemyContext;
        }

        public override void EnterState()
        {
            if (_enemyContext == null || _enemyContext.patrolZones.Count == 0)
            {
                Debug.LogWarning("Enemy không có Patrol Zone nào để đi tuần!", _enemyContext.gameObject);
                // Nếu không có zone, có thể cho nó đứng im (Idle)
                return;
            }

            // Chọn ngẫu nhiên một khu vực từ danh sách
            int randomIndex = Random.Range(0, _enemyContext.patrolZones.Count);
            _enemyContext.currentZone = _enemyContext.patrolZones[randomIndex];

            // Reset bộ đếm số lần đi tuần trong phòng
            _enemyContext.localPatrolsDone = 0;

            Debug.Log($"Enemy đã chọn khu vực mới: {_enemyContext.currentZone.zoneName}");
        }

        public override void ExitState() { }

        public override void UpdateState() { }

        // Vừa vào là chuyển ngay sang GuardZoneState để bắt đầu tuần tra trong phòng
        public override NpcStateMachine.ENpcStates GetNextState()
        {
            if (_enemyContext.currentZone == null)
            {
                // Nếu không có zone nào, quay về Idle
                return NpcStateMachine.ENpcStates.Idle;
            }
            return NpcStateMachine.ENpcStates.GuardZone;
        }
    }
}