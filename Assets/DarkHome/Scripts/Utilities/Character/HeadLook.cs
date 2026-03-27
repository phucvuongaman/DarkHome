// using System;
// using UnityEngine;
// using UnityEngine.InputSystem;



// namespace DarkHome
// {
//     /// <summary>
//     /// Class này dùng để điều khiển góc nhìn của đầu nhân vật về phía mục tiêu.
//     /// </summary>
//     public class HeadLook : MonoBehaviour
//     {
//         [Header("Runtime")]
//         [Tooltip("Đánh dấu (check) vào đây nếu bạn muốn script này tự chạy (dùng cho MainMenu). Bỏ trống (uncheck) nếu nó được gọi bởi một State Machine (dùng cho NPC).")]
//         [SerializeField] private bool _runInUpdate = false;

//         [Header("Target")]
//         [SerializeField] private Transform _target;

//         [Header("Scanner Position")]
//         [SerializeField] private Transform _headTransform;


//         [Header("View Settings")]
//         [SerializeField] private float _viewDistance = 5f;
//         [SerializeField] private float _viewAngle = 90f;
//         [Header("Head Look Range")]
//         [SerializeField] private float maxHorizontalAngle = 45f; // trái phải
//         [SerializeField] private float maxVerticalAngle = 45f;   // trên dưới


//         [Header("Smooth Settings")]
//         [SerializeField] private float _rotationSpeed = 5f;


//         // private bool isLooking;
//         private Animator _animator;

//         private readonly int hashLookX = Animator.StringToHash("LookX");
//         private readonly int hashLookY = Animator.StringToHash("LookY");

//         public Transform Target { get => _target; set => _target = value; }

//         private void Start()
//         {
//             _animator = GetComponent<Animator>();

//         }

//         private void Update()
//         {
//             if (_runInUpdate)
//             {
//                 CheckingTarget();
//             }
//         }

//         public void CheckingTarget()
//         {
//             if (_target == null) return;

//             Vector3 directionToTarget = _target.position - _headTransform.position;
//             float distanceToTarget = directionToTarget.magnitude;

//             // Kiểm tra khoảng cách
//             if (distanceToTarget > _viewDistance)
//             {
//                 // isLooking = false;
//                 UpdateAnimator(0, 0); // Reset
//                 return;
//             }

//             // Tính góc giữa forward của NPC và target
//             Vector3 direction = directionToTarget.normalized;
//             float angleToTarget = Vector3.Angle(transform.forward, direction);

//             if (angleToTarget < _viewAngle / 2f)
//             {
//                 // isLooking = true;
//                 LookAtTarget(direction);
//             }
//             else
//             {
//                 // isLooking = false;
//                 UpdateAnimator(0, 0);
//             }

//         }

//         private void LookAtTarget(Vector3 direction)
//         {
//             // Chuyển sang không gian local của NPC
//             Vector3 localDir = transform.InverseTransformDirection(direction);

//             // float lookX = Mathf.Clamp(localDir.x * 90f, -45f, 45f);
//             // float lookY = Mathf.Clamp(localDir.y * 90f, -45f, 45f);

//             float lookX = Mathf.Clamp(localDir.x, -1f, 1f) * maxHorizontalAngle;
//             float lookY = Mathf.Clamp(localDir.y, -1f, 1f) * maxVerticalAngle;

//             UpdateAnimator(lookX, lookY);
//         }

//         private void UpdateAnimator(float x, float y)
//         {
//             float currentX = _animator.GetFloat(hashLookX);
//             float currentY = _animator.GetFloat(hashLookY);

//             float smoothX = Mathf.Lerp(currentX, x, Time.deltaTime * _rotationSpeed);
//             float smoothY = Mathf.Lerp(currentY, y, Time.deltaTime * _rotationSpeed);

//             _animator.SetFloat(hashLookX, Mathf.Round(smoothX));
//             _animator.SetFloat(hashLookY, Mathf.Round(smoothY));
//         }


//     }

// }

using UnityEngine;

namespace DarkHome
{
    public class HeadLook : MonoBehaviour
    {
        [Header("Runtime Control")]
        [Tooltip("Tích vào đây nếu muốn script tự chạy Update (dùng cho Player/Menu). Bỏ tích nếu dùng cho NPC (để StateMachine gọi).")]
        [SerializeField] private bool _runInUpdate = false;

        [Header("Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _headBone; // Kéo xương Head (hoặc Neck) vào đây
        [SerializeField] private float _viewDistance = 10f;
        [SerializeField] private float _viewAngle = 160f; // Góc nhìn rộng để NPC linh hoạt

        [Header("Limits (Degrees)")]
        [Range(0, 90)][SerializeField] private float maxHorizontalAngle = 75f; // Quay trái/phải tối đa
        [Range(0, 90)][SerializeField] private float maxVerticalAngle = 50f;   // Ngước lên/xuống tối đa

        [Header("Smoothing")]
        [SerializeField] private float _rotationSpeed = 5f;

        private Animator _animator;
        private readonly int hashLookX = Animator.StringToHash("LookX");
        private readonly int hashLookY = Animator.StringToHash("LookY");

        // Biến lưu giá trị hiện tại để Lerp mượt mà
        private float _currentLookX;
        private float _currentLookY;

        public Transform Target { get => _target; set => _target = value; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            // Tự động tìm xương đầu nếu quên gán (nhưng gán tay vẫn tốt hơn)
            if (_headBone == null) _headBone = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (_headBone == null) _headBone = transform;
        }

        private void Update()
        {
            if (_runInUpdate) CheckingTarget();
        }

        // Hàm này được gọi từ NPC StateMachine
        public void CheckingTarget()
        {
            if (_target == null)
            {
                ResetLook();
                return;
            }

            // Tính toán vector từ đầu tới mục tiêu
            Vector3 directionToTarget = _target.position - _headBone.position;

            // Kiểm tra khoảng cách
            if (directionToTarget.magnitude > _viewDistance)
            {
                ResetLook();
                return;
            }

            // Chuyển hướng sang không gian Local (để tính độ lệch so với mặt tiền nhân vật)
            Vector3 localDir = transform.InverseTransformDirection(directionToTarget.normalized);

            // Tính góc Yaw (Trái/Phải) bằng Atan2 (Chính xác tuyệt đối)
            // Atan2 trả về radian, nhân Rad2Deg để ra độ
            float targetYaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            // Kiểm tra xem mục tiêu có nằm trong góc nhìn (FOV) không
            if (Mathf.Abs(targetYaw) > _viewAngle / 2f)
            {
                ResetLook(); // Mục tiêu ở sau lưng hoặc quá lệch -> Không nhìn
                return;
            }

            // Tính góc Pitch (Lên/Xuống)
            // localDir.y là độ cao, dùng Asin để tính góc nghiêng
            float targetPitch = Mathf.Asin(Mathf.Clamp(localDir.y, -1f, 1f)) * Mathf.Rad2Deg;

            // Kẹp giá trị vào giới hạn cho phép
            float lookX = Mathf.Clamp(targetYaw, -maxHorizontalAngle, maxHorizontalAngle);
            float lookY = Mathf.Clamp(targetPitch, -maxVerticalAngle, maxVerticalAngle);

            // 8. Cập nhật Animator
            UpdateAnimator(lookX, lookY);
        }

        private void ResetLook()
        {
            UpdateAnimator(0, 0);
        }

        private void UpdateAnimator(float targetX, float targetY)
        {
            // Dùng Lerp để đầu quay từ từ, không bị giật cục
            _currentLookX = Mathf.Lerp(_currentLookX, targetX, Time.deltaTime * _rotationSpeed);
            _currentLookY = Mathf.Lerp(_currentLookY, targetY, Time.deltaTime * _rotationSpeed);

            _animator.SetFloat(hashLookX, _currentLookX);
            _animator.SetFloat(hashLookY, _currentLookY);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_headBone == null) return;

            // Vẽ tầm xa
            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawWireSphere(_headBone.position, _viewDistance);

            // Vẽ góc nhìn (FOV)
            Vector3 forward = transform.forward;
            Vector3 leftLimit = Quaternion.Euler(0, -_viewAngle / 2f, 0) * forward;
            Vector3 rightLimit = Quaternion.Euler(0, _viewAngle / 2f, 0) * forward;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_headBone.position, leftLimit * 2f);
            Gizmos.DrawRay(_headBone.position, rightLimit * 2f);

            // Vẽ đường nối tới target
            if (_target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_headBone.position, _target.position);
            }
        }
#endif
    }
}


