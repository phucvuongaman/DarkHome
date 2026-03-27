using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace DarkHome
{
    public class InputManager : MonoBehaviour, PlayerInputActions.IPlayerActions, PlayerInputActions.IUIActions
    {
        private PlayerInputActions _inputActions;

        #region InputManager Instance
        public static InputManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Logic khởi tạo cũ của ông
            _inputActions = new PlayerInputActions();
            _inputActions.Player.SetCallbacks(this);
            _inputActions.UI.SetCallbacks(this);
        }
        #endregion

        // --- CÁC KÊNH PHÁT SÓNG ---
        public static event Action<Vector2> onMove;
        public static event Action<Vector2> onLook;
        public static event Action<Vector2> onPoint;
        public static event Action onJumpPressed;
        public static event Action onInteractPressed;
        public static event Action onInteractHoldStarted; // Khi bắt đầu nhấn giữ
        public static event Action onInteractHoldCanceled; // Khi thả phím ra
        public static event Action onSprintStarted; // Khi bắt đầu nhấn giữ
        public static event Action onSprintCanceled; // Khi thả phím ra
        public static event Action onLeftMousePressed;
        public static event Action onRightMousePressed;
        public static event Action onMiddleMousePressed;
        public static event Action onAttackPressed;
        public static event Action onTabPressed;
        public static event Action onEscapePressed;
        public static event Action onJournalPressed;


        private void OnEnable()
        {
            if (_inputActions == null) _inputActions = new PlayerInputActions();

            GameManager.OnGameStateChanged += HandleGameStateChanged;

            // Mặc định bật Player input khi start
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            DisableAllInputs();
        }


        private void DisableAllInputs()
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Disable();
        }

        // --- 2. DÙNG CHO CUTSCENE ---
        // Hàm này cho phép bên ngoài can thiệp bật/tắt
        public void TogglePlayerInput(bool enable)
        {
            if (enable)
            {
                _inputActions.Player.Enable();
                _inputActions.UI.Disable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                _inputActions.Player.Disable();
                _inputActions.UI.Disable(); // Tắt hết để ngồi xem phim
                Cursor.lockState = CursorLockMode.Locked; // Vẫn khóa chuột để không bay ra ngoài
                Cursor.visible = false;
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            // // "Tắt" (Disable) tất cả trước
            _inputActions.Player.Disable();
            _inputActions.Player.RemoveCallbacks(this);
            _inputActions.UI.Disable();
            _inputActions.UI.RemoveCallbacks(this);

            // "Bật" (Enable) map CHÍNH XÁC
            if (newState == GameState.Gameplay)
            {
                _inputActions.Player.Enable(); // Chỉ bật "Player"
                _inputActions.Player.SetCallbacks(this);
            }
            else // (Paused hoặc MainMenu)
            {
                _inputActions.UI.Enable();     // Chỉ bật "UI"
                _inputActions.UI.SetCallbacks(this);
            }

        }

        // Hàm này sẽ TỰ ĐỘNG được gọi khi Action "Move" được kích hoạt
        public void OnMove(InputAction.CallbackContext context)
        {
            // context.ReadValue<Vector2>() sẽ đọc giá trị Vector2 của phím WASD
            // ?.Invoke() sẽ phát đi sự kiện, script nào đăng ký sẽ nhận được
            onMove?.Invoke(context.ReadValue<Vector2>());
        }

        // Tương tự cho Jump
        public void OnJump(InputAction.CallbackContext context)
        {
            // context.performed nghĩa là khi phím được nhấn xuống
            if (context.performed)
            {
                onJumpPressed?.Invoke();
            }
        }

        // Tương tự cho Sprint
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started) // context.started là khi bắt đầu nhấn và giữ
            {
                onSprintStarted?.Invoke();
            }
            else if (context.canceled) // context.canceled là khi thả phím ra
            {
                onSprintCanceled?.Invoke();
            }
        }

        // Tương tự cho Interact
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onInteractPressed?.Invoke();
            }
        }

        // Bạn sẽ phải implement tất cả các hàm trong interface IPlayerActions
        // Nếu không dùng, bạn có thể để trống
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onAttackPressed?.Invoke();
                onLeftMousePressed?.Invoke();
            }
        }

        public void OnBlock(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onRightMousePressed?.Invoke();
            }
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onEscapePressed?.Invoke();
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {

        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onLeftMousePressed?.Invoke();
            }

        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        public void OnInteractHold(InputAction.CallbackContext context)
        {
            if (context.started) // context.started là khi bắt đầu nhấn và giữ
            {
                onInteractHoldStarted?.Invoke();
            }
            else if (context.canceled) // context.canceled là khi thả phím ra
            {
                onInteractHoldCanceled?.Invoke();
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            onLook?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onMiddleMousePressed?.Invoke();
            }
        }



        public void OnNavigate(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            onLook?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onRightMousePressed?.Invoke();
            }

        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
        }



        public void OnSubmit(InputAction.CallbackContext context)
        {
        }

        public void OnTab(InputAction.CallbackContext context)
        {
            // context.performed nghĩa là khi phím được nhấn xuống
            if (context.performed)
            {
                onTabPressed?.Invoke();
            }
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
        }

        public void OnJournal(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                onJournalPressed?.Invoke();
            }
        }


    }

}
