using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DarkHome
{

    public class MainMenuAdapter : MonoBehaviour
    {
        [SerializeField] private SlimUI.ModernMenu.UIMenuManager assetMenuManager;

        [Header("Panel References")]
        [Tooltip("Chapter Selection panel - để toggle on/off")]
        [SerializeField] private GameObject _chapterSelectionPanel;

        [Header("Audio Reference")]
        [SerializeField] private AudioClip _menuThemeClip;



        private void Start()
        {
            // Bật nhạc ở menu
            if (AudioManager.Instance != null && _menuThemeClip != null)
            {
                AudioManager.Instance.PlayMusic(_menuThemeClip);
            }
        }


        #region === Menu Navigation Functions ===

        // Settings logic handled by Controllers
        // GameSettingsController, VideoSettingsController, ControlsSettingsController
        // handle ALL setting changes. MainMenuAdapter chỉ lo navigation.

        // Hàm này sẽ được gọi bởi nút "PLAY"
        public void OnPlayButtonClicked()
        {
            // Hide Chapter Selection nếu đang mở
            DisableChapterSelection();

            // Ra lệnh cho asset làm hiệu ứng (hiện ra panel New Game, Continue...)
            if (assetMenuManager != null)
            {
                assetMenuManager.PlayCampaign();
            }
        }

        // Hàm này sẽ được gọi bởi nút "SETTINGS"
        public void OnSettingsButtonClicked()
        {
            // Hide Chapter Selection nếu đang mở
            DisableChapterSelection();

            // Ra lệnh cho asset làm hiệu ứng (di chuyển camera, chơi âm thanh)
            if (assetMenuManager != null)
            {
                assetMenuManager.Position2();
                assetMenuManager.PlaySwoosh();
            }
        }

        // Hàm này sẽ được gọi bởi nút "EXIT" trên menu chính
        public void OnExitButtonClicked()
        {
            // Hide Chapter Selection nếu đang mở
            DisableChapterSelection();

            // Ra lệnh cho asset làm hiệu ứng (hiện ra panel xác nhận "Are you sure?")
            if (assetMenuManager != null)
            {
                assetMenuManager.AreYouSure();
            }
        }

        // Hàm này sẽ được gọi bởi nút "RETURN" trong panel Settings
        public void OnReturnFromSettingsClicked()
        {
            // Hide Chapter Selection nếu đang mở
            DisableChapterSelection();

            if (assetMenuManager != null)
            {
                assetMenuManager.Position1(); // Lệnh cho camera quay về
                assetMenuManager.PlaySwoosh();
            }
        }

        /// <summary>
        /// Toggle Chapter Selection panel (gọi từ nút Chapter Select)
        /// </summary>
        public void OnChapterSelectClicked()
        {
            if (_chapterSelectionPanel.IsUnityNull()) return;

            // Toggle on/off
            bool isActive = _chapterSelectionPanel.activeSelf;
            _chapterSelectionPanel.SetActive(!isActive);

            // Play sound nếu đang mở
            if (!isActive && assetMenuManager != null)
            {
                assetMenuManager.PlaySwoosh();
            }
        }

        /// <summary>
        /// Disable Chapter Selection panel (gọi từ các nút khác)
        /// </summary>
        public void DisableChapterSelection()
        {
            if (!_chapterSelectionPanel.IsUnityNull())
            {
                _chapterSelectionPanel.SetActive(false);
            }
        }
        #endregion


        #region == Các hàm Logic Game (Do các nút con gọi) ==
        // Hàm này sẽ được gọi bởi nút "New Game"
        public void NewGame()
        {
            // Chỉ cần gọi 1 dòng duy nhất
            ChapterManager.Instance.StartNewGame();
        }


        // Hàm Load Game từ Main Menu 
        public void LoadGame()
        {
            // Đọc file save vào memory
            SaveData data = SaveLoadManager.Instance.LoadFileIntoMemory();

            if (data == null)
            {
                Debug.LogError("❌ Load thất bại: Không tìm thấy file save!");
                // TODO: Hiện popup thông báo cho player
                return;
            }

            //  Validate dữ liệu
            if (string.IsNullOrEmpty(data.currentChapterId))
            {
                Debug.LogError("❌ File save bị hỏng: Thiếu currentChapterId!");
                return;
            }

            if (string.IsNullOrEmpty(data.currentSceneName))
            {
                Debug.LogError("❌ File save bị hỏng: Thiếu currentSceneName!");
                return;
            }

            //  Tìm ChapterDataSO tương ứng
            ChapterDataSO chapterToLoad = ChapterManager.Instance.GetChapterSOById(data.currentChapterId);

            if (chapterToLoad == null)
            {
                Debug.LogError($"❌ Không tìm thấy ChapterDataSO với ID: {data.currentChapterId}");
                return;
            }

            //  Load dữ liệu vào các Manager (thứ tự quan trọng!)
            ChapterManager.Instance.LoadData(data);
            FlagManager.Instance.LoadData(data);
            DialogueTrackingState.Instance.LoadData(data);

            Debug.Log($"✅ Load Game thành công! Chapter: {chapterToLoad.chapterName}, Scene: {data.currentSceneName}");

            //  Chuyển cảnh với spawnID đặc biệt
            SceneChangeData sceneData = new SceneChangeData
            {
                SceneName = data.currentSceneName,
                TargetSpawnID = "LOAD_FROM_SAVE" // Flag: Spawn tại vị trí đã lưu
            };
            EventManager.Notify(GameEvents.SceneTransition.OnSceneChangeRequested, sceneData);
        }


        // Hàm này sẽ được gọi bởi nút "YES" trong panel xác nhận thoát game
        public void ConfirmQuitGame()
        {
            // Gọi hàm của asset để nó tự xử lý việc thoát game
            if (assetMenuManager != null)
            {
                assetMenuManager.QuitGame();
            }
        }

        // Hàm này sẽ được gọi bởi nút "Quit"
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        #endregion

    }
}