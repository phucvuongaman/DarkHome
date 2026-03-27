using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DarkHome
{
    public class GameOverManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _gameOverPanel; // Kéo cái Panel đen vào đây
        [SerializeField] private Button _retryButton;       // Nút Thử lại
        [SerializeField] private Button _quitButton;        // Nút Thoát

        private void Awake()
        {
            // Tự động tắt bảng khi vào game
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

            // Gắn sự kiện cho nút (đỡ phải kéo thả OnClick bằng tay)
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetryClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnEnable()
        {
            // Lắng nghe sự kiện: Player Chết
            EventManager.AddObserver(GameEvents.Player.OnPlayerDied, HandlePlayerDeath);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(GameEvents.Player.OnPlayerDied, HandlePlayerDeath);
        }

        // --- KHI CHẾT ---
        private void HandlePlayerDeath()
        {
            // Debug.Log(" PLAYER DIED! Hiện bảng Game Over.");

            // Dừng thời gian (Để quái ngừng đánh, Player ngừng chạy)
            Time.timeScale = 0f;

            // Mở khóa chuột (Để bấm nút)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Hiện Panel
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
        }

        // --- KHI BẤM NÚT RETRY ---
        private void OnRetryClicked()
        {
            // Debug.Log(" Đang tải lại Save gần nhất...");
            Time.timeScale = 1f; // Trả lại thời gian trước khi load

            // BƯỚC 1: Load dữ liệu từ ổ cứng lên RAM
            SaveData loadedData = SaveLoadManager.Instance.LoadFileIntoMemory();

            if (loadedData != null)
            {
                // BƯỚC 2: Gửi lệnh chuyển cảnh với mật khẩu "LOAD_FROM_SAVE"
                // Hệ thống SceneTransitionManager sẽ biết là phải load vị trí cũ
                SceneChangeData sceneData = new SceneChangeData
                {
                    SceneName = loadedData.currentSceneName,
                    TargetSpawnID = "LOAD_FROM_SAVE"
                };

                EventManager.Notify(GameEvents.SceneTransition.OnSceneChangeRequested, sceneData);
            }
            else
            {
                Debug.LogWarning("⚠️ Không tìm thấy file save! Reload lại Scene hiện tại tạm thời.");
                string currentScene = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(currentScene);
            }

            // Ẩn panel đi
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        }

        private void OnQuitClicked()
        {
            Time.timeScale = 1f;
            Debug.Log("Về Main Menu");
            SceneManager.LoadScene("Main Menu"); // Đảm bảo tên Scene Menu đúng nha
        }
    }
}