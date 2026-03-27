using Unity.Cinemachine;
using UnityEngine;


namespace DarkHome
{
    public class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Tốc độ đi bộ của player")]
        [SerializeField] private float _walkSpeed; // Tốc độ đi bộ của player
        [Tooltip("Tốc độ chạy của player")]
        [SerializeField] private float _runSpeed; // Tốc độ chạy của player
        [Tooltip("Gia tốc của player")]
        [SerializeField] private float _acceleration; // Gia tốc của player
        [Header("Độ Nhạy Chuột")]
        [SerializeField] private float _rotationSpeed; // Tốc độ xoay của player


        private PlayerContext _context;
        private Rigidbody _rb;
        // Các biến nội bộ cho việc tính toán
        private float _currentSpeed = 0f;
        private Vector3 _camForward;
        private Vector3 _camRight;
        private Vector3 _moveDirection;



        private readonly int hashMoveSpeed = Animator.StringToHash("MoveSpeed");
        private readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");
        private readonly int hashJump = Animator.StringToHash("Jump");
        private readonly int hashMoveHorizontal = Animator.StringToHash("MoveHorizontal");
        private readonly int hashMoveVertical = Animator.StringToHash("MoveVertical");


        private void Awake()
        {
            // Chỉ cần lấy Context, nó chứa mọi thứ khác chúng ta cần
            _context = GetComponent<PlayerContext>();
            _rb = GetComponent<Rigidbody>();

        }


        private void FixedUpdate()
        {
            // Luôn đọc trạng thái hiện tại từ Context
            var currentState = _context.StateMachine.CurrentStateKey;

            // Chỉ cho phép di chuyển và xoay khi ở trạng thái phù hợp
            if (currentState == PlayerStateMachine.EPlayerState.Idle || currentState == PlayerStateMachine.EPlayerState.Move)
            {
                CacheCameraVectors();
                HandleRotation();
                HandleMovement();
            }
            else
            {
                // Khi không di chuyển được (ví dụ: đang nói chuyện), đảm bảo tốc độ về 0
                HandleDeceleration();
            }
        }

        /// <summary>
        /// Lấy và lưu trữ các vector hướng của camera.
        /// </summary>
        private void CacheCameraVectors()
        {
            if (CameraController.Instance.PlayerCam == null) return;

            _camForward = CameraController.Instance.PlayerCam.transform.forward;
            _camForward.y = 0;
            _camForward.Normalize();

            _camRight = CameraController.Instance.PlayerCam.transform.right;
            _camRight.y = 0;
            _camRight.Normalize();
        }

        /// <summary>
        /// Phương thức này xoay player theo hướng di chuyển dựa trên hướng camera.
        /// Nó sẽ lấy hướng camera, loại bỏ thành phần y để chỉ sử dụng hướng ngang, và sau đó xoay player theo hướng đó.
        /// </summary>
        private void HandleRotation()
        {
            if (_camForward == Vector3.zero) return; // Không xoay nếu không có hướng camera     
            Quaternion toRotation = Quaternion.LookRotation(_camForward, Vector3.up);
            _context.Rb.MoveRotation(Quaternion.Slerp(transform.rotation, toRotation, _rotationSpeed * Time.fixedDeltaTime));
        }

        /// <summary>
        /// Phương thức này xử lý di chuyển của player dựa trên đầu vào từ PlayerInput.
        /// Nó sẽ cập nhật tốc độ di chuyển, xoay player theo hướng di chuyển và áp dụng lực di chuyển lên Rigidbody.
        /// </summary>
        private void HandleMovement()
        {
            // Đọc MoveInput và IsSprinting từ Context
            Vector2 moveInput = _context.MoveInput;
            // Debug.Log("moveInput " + moveInput);

            _moveDirection = _camForward * moveInput.y + _camRight * moveInput.x;
            // Debug.Log("_moveDirection.magnitude " + _moveDirection.magnitude);
            if (_moveDirection.magnitude > 0.01f)
            {
                bool isSprinting = _context.IsSprinting;

                // Cập nhật Animator
                _context.Animator.SetFloat(hashMoveHorizontal, moveInput.x, 0.1f, Time.fixedDeltaTime);
                _context.Animator.SetFloat(hashMoveVertical, moveInput.y, 0.1f, Time.fixedDeltaTime);

                // Tính toán tốc độ
                float targetSpeed = isSprinting ? _runSpeed : _walkSpeed;
                _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _acceleration * Time.fixedDeltaTime);

                // Di chuyển Rigidbody
                _context.Rb.MovePosition(_context.Rb.position + _moveDirection * _currentSpeed * Time.fixedDeltaTime);
                _context.Animator.SetFloat(hashMoveSpeed, _currentSpeed);
            }
            else
            {
                HandleDeceleration();
            }
        }

        private void HandleDeceleration()
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _acceleration * Time.fixedDeltaTime);
            _context.Animator.SetFloat(hashMoveSpeed, _currentSpeed);
        }

        public void EnableCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }


        private void HandleJump()
        {
            if (IsOnGround() && _context.JumpTriggered)
            {
                _context.Animator.SetTrigger(hashJump);
                _context.JumpTriggered = false; // "TIÊU THỤ" CỜ NGAY TẠI ĐÂY
            }
            _context.Animator.SetBool(hashIsGrounded, IsOnGround());
        }
        private bool IsOnGround()
        {
            float checkDistance = 0.2f;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            return Physics.Raycast(origin, Vector3.down, checkDistance);
        }
    }
}
