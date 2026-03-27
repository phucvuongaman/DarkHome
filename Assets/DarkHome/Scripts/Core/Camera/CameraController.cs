using Unity.Cinemachine;
using UnityEngine;

namespace DarkHome
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Cinemachine Components")]
        [SerializeField] private CinemachineInputAxisController _camInputAxisController;
        [SerializeField] private CinemachineCamera _playerCam;
        [SerializeField] private Camera _unityCamera;
        [SerializeField] private AudioListener _audioListener;

        [Header("Effects Settings")]
        [SerializeField] private float _baseFOV = 60f;
        [SerializeField] private float _sanityFovRange = 10f;
        [SerializeField] private float _shakeAmount = 0.2f;
        [SerializeField] private float _lerpSpeed;

        private PlayerContext _playerContext;
        private Vector3 _originalCamPos;


        public CinemachineCamera PlayerCam { get => _playerCam; }
        public Camera UnityCamera => _unityCamera;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleCameraOnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnEnable()
        {
            // Lắng nghe các sự kiện quan trọng
            EventManager.AddObserver<bool>(GameEvents.Camera.EnableCamRotate, EnableCamRotate);
            EventManager.AddObserver<float>(GameEvents.Player.OnSanityChanged, ApplySanityEffect);
            EventManager.AddObserver<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            GameManager.OnGameStateChanged += HandleCameraOnGameStateChanged;
        }

        private void OnDisable()
        {
            // Hủy lắng nghe
            EventManager.RemoveListener<bool>(GameEvents.Camera.EnableCamRotate, EnableCamRotate);
            EventManager.RemoveListener<float>(GameEvents.Player.OnSanityChanged, ApplySanityEffect);
            EventManager.RemoveListener<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            GameManager.OnGameStateChanged -= HandleCameraOnGameStateChanged;

        }


        private void Update()
        {
            if (_playerContext == null) return;

            // Logic shake chỉ chạy khi cần thiết, không cần gọi từ bên ngoài
            if (_playerCam.Follow != null && _playerContext.Stats.Sanity < 25f)
            {
                Vector3 shake = Random.insideUnitSphere * _shakeAmount * Time.deltaTime;
                _playerCam.transform.localPosition = _originalCamPos + shake;
            }
            else if (_playerCam.transform.localPosition != _originalCamPos)
            {
                _playerCam.transform.localPosition = Vector3.Lerp(_playerCam.transform.localPosition
                , _originalCamPos, Time.deltaTime * _lerpSpeed);
            }
        }

        // Hàm này giờ sẽ được gọi MỘT LẦN khi Player được spawn
        private void HandlePlayerSpawned(Transform playerTransform)
        {
            if (playerTransform == null) return;

            _playerContext = playerTransform.GetComponent<PlayerContext>();
            if (_playerContext == null)
            {
                Debug.LogError("Spawned Player does not have a PlayerContext component!", playerTransform);
                return;
            }

            CameraAnchorMarker anchorMarker = playerTransform.GetComponentInChildren<CameraAnchorMarker>();
            if (anchorMarker != null)
            {
                Transform cameraAnchor = anchorMarker.transform;
                _playerCam.Follow = cameraAnchor;
                _originalCamPos = _playerCam.transform.localPosition;
                Debug.Log("Camera Target and Context SET!");
            }
            else
            {
                Debug.LogError("Could not find 'Camera Anchor' as a child of the new Player.", playerTransform);
            }
        }

        // Hàm này giờ chỉ được gọi KHI sanity thực sự thay đổi
        private void ApplySanityEffect(float sanityValue)
        {
            float t = Mathf.InverseLerp(100f, 0f, sanityValue);
            float fovOffset = Mathf.Lerp(0f, _sanityFovRange, t);
            _playerCam.Lens.FieldOfView = _baseFOV + fovOffset;
        }



        private void EnableCamRotate(bool active)
        {
            if (_camInputAxisController != null)
            {
                _camInputAxisController.enabled = active;
            }
        }

        private void HandleCameraOnGameStateChanged(GameState newState)
        {
            if (newState == GameState.MainMenu)
            {
                // Tắt Cinemachine (Logic cũ)
                if (_playerCam != null) _playerCam.enabled = false;

                // Tắt luôn Camera Unity (để tránh xung đột với Cam của Menu)
                if (_unityCamera != null) _unityCamera.enabled = false;

                // Tắt AudioListener (để tránh lỗi "2 Audio Listeners")
                if (_audioListener != null) _audioListener.enabled = false;
            }
            else // Gameplay, Paused...
            {
                // Bật lại tất cả khi vào game
                if (_playerCam != null) _playerCam.enabled = true;
                if (_unityCamera != null) _unityCamera.enabled = true;
                if (_audioListener != null) _audioListener.enabled = true;
            }
        }
    }
}