// Kế thừa NPC nên có mọi thứ mà NPC có.
// Thêm hàm này vào go của một Enemy

using System.Collections.Generic;
using UnityEngine;


namespace DarkHome
{
    [RequireComponent(typeof(EnemyMove), typeof(EnemyRage), typeof(EnemyAudio))]
    public class EnemyContext : NpcContext // Kế thừa từ NpcContext
    {
        public EnemyMove EnemyMove { get; private set; }
        public EnemyRage EnemyRage { get; private set; }
        public EnemyAudio EnemyAudio { get; private set; }

        [Header("Patrol Settings")]
        public List<PatrolZone> patrolZones; // Danh sách các khu vực

        // Các biến này sẽ do State Machine quản lý, nhưng chúng ta có thể giữ lại để debug
        [HideInInspector] public PatrolZone currentZone;
        [HideInInspector] public int localPatrolsDone = 0;

        protected override void OnPlayerSpawned(Transform player)
        {
            base.OnPlayerSpawned(player);
            EnemyMove.Target = player;
        }

        protected override void Awake()
        {
            base.Awake();

            //  Lấy các component của riêng Enemy
            EnemyMove = GetComponent<EnemyMove>();
            EnemyRage = GetComponent<EnemyRage>();
            EnemyAudio = GetComponent<EnemyAudio>();
        }
    }
}