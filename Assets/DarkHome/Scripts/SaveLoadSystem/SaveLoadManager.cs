using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DarkHome
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }


        [Header("Settings")]
        [SerializeField] private string _fileName = "savegame.json";

        private string _saveFilePath;

        // --- CÁI TÚI THẦN KỲ (Lưu trong RAM) ---
        private SaveData _saveData;

        private List<IDataPersistence> _dataPersistenceObjects;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        }

        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            // Tìm tất cả object kể cả đang ẩn (Inactive) để đảm bảo không sót
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IDataPersistence>().ToList();
        }

        public void NewGame()
        {
            _saveData = new SaveData();
            // Debug.Log("New Game Data Initialized!");
        }

        // --- GOM ĐỒ VÀO TÚI (RAM)  ---
        public void SaveSceneToMemory()
        {
            if (_saveData == null) _saveData = new SaveData();

            var persistenceObjects = FindAllDataPersistenceObjects();
            foreach (var dataPersistenceObj in persistenceObjects)
            {
                dataPersistenceObj.SaveData(ref _saveData);
            }
            _saveData.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            // Debug.Log($"[Memory] Đã gom dữ liệu của {persistenceObjects.Count} object vào túi.");
        }

        // --- HÀM LƯU RA Ổ CỨNG ---
        public void SaveGame()
        {
            SaveSceneToMemory(); // Gom đồ trước
            try
            {
                File.WriteAllText(_saveFilePath, JsonUtility.ToJson(_saveData, true));
                // Debug.Log("Game Saved to Disk: " + _saveFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e);
            }
        }

        // --- HÀM LOAD TỪ Ổ CỨNG ---
        public SaveData LoadFileIntoMemory()
        {
            if (!File.Exists(_saveFilePath)) return null;
            try
            {
                string jsonData = File.ReadAllText(_saveFilePath);
                _saveData = JsonUtility.FromJson<SaveData>(jsonData);
                return _saveData;
            }
            catch (Exception e)
            {
                Debug.LogError("Load failed: " + e);
                return null;
            }
        }

        public void CompleteChapter(int nextChapterIndex)
        {
            // 1. Lưu game hiện tại lại trước
            SaveGame();

            // 2. Tạo đường dẫn backup
            string currentSavePath = _saveFilePath; // Dùng luôn biến đã cache
            string checkpointPath = GetCheckpointPath(nextChapterIndex);

            try
            {
                if (File.Exists(currentSavePath))
                {
                    File.Copy(currentSavePath, checkpointPath, true); // true = ghi đè
                    Debug.Log($" Đã tạo Checkpoint cho Chapter {nextChapterIndex} tại: {checkpointPath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi tạo Checkpoint: {e.Message}");
            }
        }

        public void ReplayFromChapter(int chapterIndex)
        {
            string checkpointPath = GetCheckpointPath(chapterIndex);
            string currentSavePath = _saveFilePath;

            if (File.Exists(checkpointPath))
            {
                try
                {
                    // 1. Lấy file checkpoint cũ đè bẹp file hiện tại
                    File.Copy(checkpointPath, currentSavePath, true);
                    // Debug.Log($" Đã tua ngược thời gian về Chapter {chapterIndex}.");

                    // 2. Load lại dữ liệu từ ổ cứng vào RAM ngay lập tức
                    LoadFileIntoMemory(); // ✅ SỬA: Gọi đúng tên hàm này

                    // 3. Báo cho Scene biết để cập nhật đồ vật (nếu đang ở trong game)
                    ApplyLoadedDataToScene();
                }
                catch (Exception e)
                {
                    Debug.LogError($" Lỗi Replay Chapter: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Không tìm thấy Checkpoint của Chapter {chapterIndex}! Bạn chưa chơi tới đây bao giờ.");
            }
        }

        private string GetCheckpointPath(int chapterIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"checkpoint_chapter_{chapterIndex}.json");
        }

        // ---  HÀM BÀY ĐỒ (FIX CRASH)  ---
        public void ApplyLoadedDataToScene()
        {
            // NẾU TÚI RỖNG -> KHÔNG LÀM GÌ CẢ (Tránh lỗi NullReference)
            if (_saveData == null)
            {
                Debug.LogWarning("[SaveLoadManager] Túi rỗng (_saveData is null). Không có gì để load.");
                return;
            }

            var persistenceObjects = FindAllDataPersistenceObjects();
            foreach (var dataPersistenceObj in persistenceObjects)
            {
                dataPersistenceObj.LoadData(_saveData);
            }

            //  Refresh Quest UI SAU KHI tất cả flags đã được load
            // FlagManager.LoadData chạy trước → flags có sẵn → QuestObjective.IsCompleted đúng
            if (QuestManager.Instance != null)
                QuestManager.Instance.RefreshQuestUI();

            Debug.Log("📥 [SaveLoadManager] Đã bày đồ ra Scene mới.");
        }

        // Thêm hàm này vào cuối class SaveLoadManager
        public void IncreaseDayCount()
        {
            if (_saveData == null) return;

            _saveData.currentDay++;

            // Không tự grant flag ở đây — Bed.cs đã grant C1_PROGRESS_DAY{N}_COMPLETE
            // Event system sẽ tự chain: COMPLETE → STARTED qua Events.csv
            // Chỉ notify ActiveByDay để show/hide objects đúng ngày
            EventManager.Notify<int>(GameEvents.Day.OnDayChanged, _saveData.currentDay);
            Debug.Log($"[SaveLoadManager] Day → {_saveData.currentDay}");
        }

        public int GetCurrentDay()
        {
            return _saveData != null ? _saveData.currentDay : 1;
        }
    }
}
