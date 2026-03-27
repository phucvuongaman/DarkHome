using UnityEngine;
using UnityEngine.AI;

namespace DarkHome
{
    [RequireComponent(typeof(NpcContext))]
    public class NpcMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _runSpeed = 6f;
        [SerializeField] private float _lerpSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 15f; // Tốc độ xoay thủ công

        private NpcContext _context;
        // private Animator _animator;
        private float _currentSpeed = 0f;

        // Các thuộc tính tiện ích
        public bool IsMoving => _context.Agent.velocity.sqrMagnitude > 0.01f;
        public bool IsIdle => !IsMoving;
        public bool HasReachedDestination => !_context.Agent.pathPending &&
                                             _context.Agent.remainingDistance <= _context.Agent.stoppingDistance;

        private readonly int hashMoveSpeed = Animator.StringToHash("MoveSpeed");

        private void Awake()
        {
            // _animator = GetComponent<Animator>();
            _context = GetComponent<NpcContext>();
        }

        public void StopMove()
        {
            // Dừng Agent vật lý (Kéo phanh tay)
            if (_context.Agent.isOnNavMesh && !_context.Agent.isStopped)
            {
                _context.Agent.isStopped = true;
                _context.Agent.ResetPath();
                _context.Agent.velocity = Vector3.zero; // Dừng hẳn quán tính
            }

            // Giảm Animation về 0
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _lerpSpeed * Time.deltaTime);
            _context.Animator.SetFloat(hashMoveSpeed, _currentSpeed);
        }

        public void MoveToTarget(Vector3 targetPosition)
        {
            if (_context.Agent.isOnNavMesh)
            {
                _context.Agent.isStopped = false;
                _context.Agent.updateRotation = true; // Trả quyền xoay cho Agent
                _context.Agent.updatePosition = true;
                _context.Agent.SetDestination(targetPosition);
            }
        }

        // Xoay người thủ công (Dùng khi đứng yên nói chuyện)
        public void RotateTowards(Vector3 targetPosition, float speed = -1)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0; // Chỉ xoay ngang

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Nếu không truyền tốc độ thì lấy mặc định
                float finalSpeed = (speed < 0) ? _rotationSpeed : speed;

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * finalSpeed);
            }
        }

        // Cập nhật Animation Đi bộ
        public void AnimatorWalk()
        {
            _context.Agent.speed = _moveSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 1f, _lerpSpeed * Time.deltaTime);
            _context.Animator.SetFloat(hashMoveSpeed, _currentSpeed);
        }

        // Cập nhật Animation Chạy
        public void AnimatorRun()
        {
            _context.Agent.speed = _runSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 2f, _lerpSpeed * Time.deltaTime);
            _context.Animator.SetFloat(hashMoveSpeed, _currentSpeed);
        }
    }
}