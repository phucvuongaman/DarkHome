using UnityEngine;
using UnityEngine.AI;

namespace DarkHome
{
    public class EnemyMove : MonoBehaviour
    {
        [Header("Target to move")]
        [Tooltip("It will automatically find the target when the scene load")]
        [SerializeField] private Transform _target;

        public Transform Target { get => _target; set => _target = value; }


        private EnemyContext _context;

        void Awake()
        {

            _context = GetComponent<EnemyContext>();
        }

        public void ChasingToTarget()
        {
            _context.Agent.isStopped = false;
            _context.Agent.SetDestination(_target.position);

        }

        public void StopChasing()
        {
            _context.Agent.isStopped = true;
        }
    }
}