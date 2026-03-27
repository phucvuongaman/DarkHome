using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DarkHome
{
    // Các trạng thái của game
    public enum GameState
    {
        MainMenu,
        Gameplay,
        Paused
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }


        // Event để thông báo cho toàn bộ game biết khi trạng thái thay đổi
        public static event System.Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            // Kiểm tra: Nếu đang ở màn chơi (Gameplay) mà ChapterManager chưa có dữ liệu
            // (Nghĩa là chưa Load Game từ Save), thì tự động coi như là NEW GAME.
            if (CurrentState == GameState.Gameplay)
            {
                // Đợi 1 frame để đảm bảo các hệ thống khác (SaveLoad) đã chạy xong (nếu có)
                StartCoroutine(InitializeGameContent());
            }
        }

        private IEnumerator InitializeGameContent()
        {
            yield return null; // Chờ 1 frame

            // Logic kiểm tra cực chuẩn:
            // 1. Nếu ChapterManager tồn tại
            // 2. VÀ RuntimeQuests đang NULL (nghĩa là chưa ai nạp dữ liệu cả)
            if (ChapterManager.Instance != null && ChapterManager.Instance.RuntimeQuests == null)
            {
                Debug.Log("🎮 Không phát hiện dữ liệu Load Game -> Tự động chạy NEW GAME.");
                ChapterManager.Instance.StartNewGame();
            }
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Khi một scene mới được load, quyết định xem game đang ở trạng thái nào
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Giả sử scene Main Menu của bạn có tên là "MainMenu"
            if (scene.name == "Main Menu" || scene.name == "Boot")
            {
                UpdateGameState(GameState.MainMenu);
            }
            else // Bất kỳ scene nào khác đều là Gameplay
            {
                UpdateGameState(GameState.Gameplay);
            }
        }

        public void UpdateGameState(GameState newState)
        {
            if (newState == CurrentState) return;

            CurrentState = newState;
            Debug.Log($"Game State changed to: {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
        // public void TogglePause()
        // {
        //     if (CurrentState == GameState.Gameplay)
        //     {
        //         UpdateGameState(GameState.Paused);
        //     }
        //     else if (CurrentState == GameState.Paused)
        //     {
        //         UpdateGameState(GameState.Gameplay);
        //     }
        // }
    }
}