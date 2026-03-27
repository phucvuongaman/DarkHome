using UnityEngine;

namespace DarkHome
{
    public class NpcMoveTrigger : MonoBehaviour
    {
        [SerializeField] private Vector3 _moveToPos;
        [SerializeField] private float _radius;
        [SerializeField] private NpcContext _npcContext;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _npcContext != null)
            {
                PlayerOnTrigger();
            }
        }

        private void PlayerOnTrigger()
        {
            // Gửi mục tiêu đến "bộ nhớ" của não
            _npcContext.StateMachine.SetMoveTarget(_moveToPos);

            // Ra lệnh cho "não" tự quyết định chuyển state
            _npcContext.StateMachine.TransitionToState(NpcStateMachine.ENpcStates.Move);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(transform.position, _radius);
            if (_moveToPos != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, _moveToPos);
                Gizmos.DrawSphere(_moveToPos, _radius);
            }
        }
#endif
    }
}
