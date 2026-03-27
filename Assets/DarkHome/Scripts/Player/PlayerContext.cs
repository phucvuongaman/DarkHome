using UnityEngine;
using System.Collections;
using UnityEngine.AI;

namespace DarkHome
{
    [RequireComponent(typeof(PlayerStateMachine), typeof(PlayerMovement), typeof(Interactor))]
    [RequireComponent(typeof(PlayerStats), typeof(SanityStateMachine), typeof(HeadLook))]
    [RequireComponent(typeof(Rigidbody), typeof(PlayerSaveAdapter))]
    public class PlayerContext : MonoBehaviour, IDataPersistence
    {
        #region Component References
        // --- CÁC THAM CHIẾU COMPONENT ---
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public Interactor Interactor { get; private set; }
        public Animator Animator { get; private set; }
        public Rigidbody Rb { get; private set; }
        public PlayerStats Stats { get; private set; }
        public SanityStateMachine SanityMachine { get; private set; }
        public HeadLook HeadLook { get; private set; }
        #endregion

        #region Input Data
        // --- NƠI LƯU TRỮ INPUT (NGUỒN SỰ THẬT) ---
        // Input Trạng thái
        public Vector2 MoveInput { get; private set; }
        public Vector2 MouseInput { get; private set; }
        public bool IsSprinting { get; private set; }


        // Input "Trigger" (nhấn 1 lần)
        public bool JumpTriggered { get; set; }
        public bool IsLeftMousePress { get; set; }
        public bool IsRightMousePress { get; set; }
        public bool IsMiddleMousePress { get; set; }
        public bool TabPressed { get; set; }
        public bool EscPressed { get; set; }
        public bool InteractPressed { get; set; }
        public bool IsInteractHolding { get; set; }
        #endregion




        private void Awake()
        {
            StateMachine = GetComponent<PlayerStateMachine>();
            Movement = GetComponent<PlayerMovement>();
            Interactor = GetComponent<Interactor>();
            Animator = GetComponentInChildren<Animator>();
            Rb = GetComponent<Rigidbody>();
            Stats = GetComponent<PlayerStats>();

            GameManager.OnGameStateChanged += HandleGameStateChanged;

            if (Animator == null) Debug.LogError("❌ KHÔNG TÌM THẤY ANIMATOR TRONG VISUALS!");
        }

        private void OnDestroy()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                // Tự mình gọi hàm xử lý với trạng thái hiện tại của game
                HandleGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        // Đây chính là hàm bạn hỏi!
        private void HandleGameStateChanged(GameState newState)
        {
            // Chỉ "làm việc" khi game ở trạng thái Gameplay
            if (newState == GameState.Gameplay)
            {
                EnablePlayerInput();
            }
            else
            {
                DisablePlayerInput();
            }
        }




        public void SaveData(ref SaveData data)
        {
            data.playerPosition = this.transform.position;
            data.playerRotation = this.transform.rotation;
        }

        public void LoadData(SaveData data)
        {
            this.Rb.position = data.playerPosition;
            this.Rb.rotation = data.playerRotation;
        }



        #region Event Register 
        // Gói tất cả các đăng ký event vào hàm riêng
        private void EnablePlayerInput()
        {
            // Debug.Log("EnablePlayerInput");
            // Đăng ký tất cả các sự kiện với các hàm được đặt tên
            InputManager.onMove += SetMoveInput;
            InputManager.onLook += SetMouseInput;
            InputManager.onSprintStarted += SetSprintTrue;
            InputManager.onSprintCanceled += SetSprintFalse;

            InputManager.onJumpPressed += SetJumpTrue;
            InputManager.onLeftMousePressed += SetLeftMouseTrue;
            InputManager.onInteractPressed += SetInteractTrue;
            InputManager.onTabPressed += SetTabTrue;
            InputManager.onEscapePressed += SetEscTrue;

            // Dọn dẹp các đăng ký thừa cho Interactor
            InputManager.onInteractPressed += HandleInteractPress;
            InputManager.onInteractHoldStarted += HandleInteractHold;

            // Lắng nghe sự kiện Save/Load
            // EventManager.AddObserver(GameEvents.Player.OnPlayerLoadData, OnPlayerLoadData);
        }

        // Gói tất cả các hủy đăng ký vào hàm riêng
        private void DisablePlayerInput()
        {
            // Hủy đăng ký tất cả các sự kiện một cách chính xác
            InputManager.onMove -= SetMoveInput;
            InputManager.onLook -= SetMouseInput;
            InputManager.onSprintStarted -= SetSprintTrue;
            InputManager.onSprintCanceled -= SetSprintFalse;

            InputManager.onJumpPressed -= SetJumpTrue;
            InputManager.onLeftMousePressed -= SetLeftMouseTrue;
            InputManager.onInteractPressed -= SetInteractTrue;
            InputManager.onTabPressed -= SetTabTrue;
            InputManager.onEscapePressed -= SetEscTrue;

            InputManager.onInteractPressed -= HandleInteractPress;
            InputManager.onInteractHoldStarted -= HandleInteractHold;

            // EventManager.RemoveListener(GameEvents.Player.OnPlayerLoadData, OnPlayerLoadData);
        }
        #endregion

        #region Input Handlers
        // --- CÁC HÀM XỬ LÝ SỰ KIỆN GỌN GÀNG ---
        private void SetMoveInput(Vector2 direction) => MoveInput = direction;
        private void SetMouseInput(Vector2 direction) => MouseInput = direction;
        private void SetSprintTrue() => IsSprinting = true;
        private void SetSprintFalse() => IsSprinting = false;
        private void SetJumpTrue() => JumpTriggered = true;
        private void SetLeftMouseTrue() => IsLeftMousePress = true;
        private void SetInteractTrue() => InteractPressed = true;
        private void SetTabTrue() => TabPressed = true;
        private void SetEscTrue() => EscPressed = true;
        private void HandleInteractPress()
        {
            if (StateMachine.CurrentStateKey == PlayerStateMachine.EPlayerState.Talk) return;
            Interactor.InteractPress();
        }

        private void HandleInteractHold()
        {
            if (StateMachine.CurrentStateKey == PlayerStateMachine.EPlayerState.Talk) return;
            Interactor.InteractHold();
        }
        #endregion

    }
}