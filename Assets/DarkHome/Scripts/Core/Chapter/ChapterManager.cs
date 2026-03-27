// KHÔNG ĐƯỢC XÓA ĐOẠN NÀY
// Hơi bò cô bê nên lưu lại flow kẻo quên
// Nghĩa là tối nêu chơi ở chapter 3 (có thể đã xong hoặc chưa xong) rồi qua chapter 1 thì 

// KỊCH BẢN 1: Đi từ C2 -> C3 (tuần tự)
// (newChapterIndex = 2)
// (2 == 1 + 1)  -> true
// -> isSequentialForward = true -> "Chốt sổ" (Commit) di sản C2.

// KỊCH BẢN 2: Đi từ C2 -> C1 (chơi lại)
// (newChapterIndex = 0)
// (0 == 1 + 1)  -> false
// -> isSequentialForward = false -> Bỏ qua Commit. "Di sản" C2 được bảo vệ.

// KỊCH BẢN 3: Đi từ C1 -> C3 (nhảy cóc)
// (Giả sử _currentChapterIndex = 0)
// (newChapterIndex = 2)
// (2 == 0 + 1)  -> false
// -> isSequentialForward = false -> Bỏ qua Commit.

// Điều này nghĩa là sẽ ngăn cho nó không ghi đè xuyên chpater 1 mà đè lên chapter 3
// Nếu chơi lại chapter 1 mà muốn thay đổi thì phải chơi thêm chapter 2 tiếp để đè chapter 3,...
// KHÔNG ĐƯỢC XÓA ĐOẠN NÀY

// Đoạn này cũ rồi, có thể không còn đúng nữa :D lười viết

using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    public class ChapterManager : MonoBehaviour, IDataPersistence
    {
        public static ChapterManager Instance { get; private set; }

        public QuestDataSO RuntimeQuests { get; private set; }
        public string CurrentChapterId { get; private set; }

        // --- CÁC BIẾN MỚI ĐỂ THEO DÕI TIẾN TRÌNH ---
        private int _currentChapterIndex = -1;

        [Header("Chapter Progression")]
        [Tooltip("Kéo các ChapterDataSO vào list này trong Inspector theo đúng thứ tự")]
        [SerializeField] private List<ChapterDataSO> _chapterProgression;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public ChapterDataSO GetChapterSOById(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId)) return null;

            // ChapterDataSO đầu tiên có `chapterId` khớp.
            return _chapterProgression.Find(c => c.chapterId == chapterId);
        }

        /// <summary>
        /// Get all chapters (for Chapter Selection UI)
        /// </summary>
        public List<ChapterDataSO> GetAllChapters()
        {
            return _chapterProgression;
        }

        /// <summary>
        /// Get highest unlocked chapter index (0 = Chapter 1 only, 1 = Chapters 1-2 unlocked, etc.)
        /// </summary>
        public int GetHighestUnlockedChapter()
        {
            if (SaveLoadManager.Instance == null) return 0;

            SaveData data = SaveLoadManager.Instance.LoadFileIntoMemory();
            if (data != null)
                return data.highestUnlockedChapter;

            return 0; // Default: Chapter 1 only
        }

        /// <summary>
        /// Unlock next chapter (call when player completes current chapter)
        /// </summary>
        public void UnlockNextChapter()
        {
            if (_currentChapterIndex < 0 || SaveLoadManager.Instance == null) return;

            int nextIndex = _currentChapterIndex + 1;
            if (nextIndex >= _chapterProgression.Count) return;

            // Load current save data
            SaveData data = SaveLoadManager.Instance.LoadFileIntoMemory() ?? new SaveData();

            // Update unlock progress if this is a new unlock
            if (data.highestUnlockedChapter < nextIndex)
            {
                data.highestUnlockedChapter = nextIndex;
                Debug.Log($"🔓 Unlocked Chapter {nextIndex + 1}: {_chapterProgression[nextIndex].chapterName}");

                // Save updated progress
                // This will persist when SaveLoadManager saves the game
            }
        }

        /// Tự động lấy Chapter đầu tiên (Index 0) để chạy.
        public void StartNewGame()
        {
            if (_chapterProgression == null || _chapterProgression.Count == 0) return;

            // Lấy chapter đầu tiên
            ChapterDataSO firstChapter = _chapterProgression[0];

            // 1. Load dữ liệu
            LoadChapter(firstChapter);

            // 2. Tự chuyển cảnh luôn! (MainMenu không cần lo nữa)
            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log($"🆕 NEW GAME: Đang vào {firstChapter.sceneName}...");
                SceneTransitionManager.Instance.TransitionTo(
                    firstChapter.sceneName,
                    firstChapter.startSpawnID
                );
            }
        }

        /// <summary>
        /// Hàm cốt lõi để tải một chapter, có thể được gọi từ New Game, Load Game, hoặc chuyển cảnh.
        /// </summary>
        public void LoadChapter(ChapterDataSO chapterData)
        {
            if (chapterData == null)
            {
                Debug.LogError("Attempted to load a null ChapterDataSO!");
                return;
            }

            Debug.Log($"Loading Chapter: {chapterData.chapterName}");

            // --- TRÁI TIM CỦA LOGIC KẾ THỪA ---

            // Tìm index của chapter mới trong chuỗi tiến trình
            int newChapterIndex = _chapterProgression.IndexOf(chapterData);
            if (newChapterIndex == -1)
            {
                Debug.LogError($"Chapter '{chapterData.chapterName}' không được đăng ký trong Chapter Progression của ChapterManager!");
                return;
            }


            // Xác định xem có phải đang đi "TIẾN" một cách "TUẦN TỰ" không
            // (tức là index mới = index cũ + 1)
            bool isSequentialForward = (newChapterIndex == _currentChapterIndex + 1);

            // "Chốt sổ" KÝ ỨC (CHỈ KHI ĐI TIẾN LÊN TUẦN TỰ)
            if (isSequentialForward)
            {
                // Chỉ "lưu di sản" nếu ta đang đi tiếp tuần tự
                FlagManager.Instance.CommitRuntimeFlagsToHistory();
            }

            // Xác định xem có phải đang chơi lại không (logic cũ vẫn đúng)
            bool isReplaying = (newChapterIndex <= _currentChapterIndex);

            // Cập nhật trạng thái hiện tại
            CurrentChapterId = chapterData.chapterId;
            _currentChapterIndex = newChapterIndex;

            // RA LỆNH CHO FLAGMANAGER
            // - Nếu đi tiến (C1->C2): `_previousChapterFlags` vừa được cập nhật (Commit).
            // - Nếu chơi lại (C3->C1): `_previousChapterFlags` được bảo vệ (không Commit).
            // - Nếu nhảy cóc (C1->C3): `_previousChapterFlags` được bảo vệ (không Commit).
            FlagManager.Instance.PrepareFlagsForNewChapter(chapterData.originFlags, isReplaying);

            // Tạo bản sao runtime cho Quests (logic này vẫn đúng)
            if (chapterData.defaultQuests != null)
            {
                RuntimeQuests = Instantiate(chapterData.defaultQuests);
            }

            // Phát tín hiệu cho các hệ thống khác (như Dialogue)
            EventManager.Notify(GameEvents.ChapterManager.OnChapterLoaded, CurrentChapterId);
        }

        // --- HÀM SAVE/LOAD ---
        public void SaveData(ref SaveData data)
        {
            data.currentChapterId = this.CurrentChapterId;
            data.currentChapterIndex = this._currentChapterIndex; // Lưu lại chapter index
            if (this.RuntimeQuests != null) data.quests = this.RuntimeQuests.quests;
        }


        // Hơi khó nhớ
        public void LoadData(SaveData data)
        {
            if (string.IsNullOrEmpty(data.currentChapterId)) return;

            ChapterDataSO savedChapter = _chapterProgression.Find(c => c.chapterId == data.currentChapterId);

            if (savedChapter == null)
            {
                Debug.LogError($"❌ Load Game: Không tìm thấy ChapterDataSO '{data.currentChapterId}'.");
                return;
            }

            Debug.Log($"📂 LOAD GAME: Khôi phục chapter '{savedChapter.chapterName}'...");

            // KHÔNG gọi LoadChapter() vì nó sẽ xóa runtimeFlags qua PrepareFlagsForNewChapter()
            // Flags sẽ được FlagManager.LoadData restore riêng.
            // Chỉ setup tối thiểu:

            // Set chapter identity
            CurrentChapterId = savedChapter.chapterId;
            _currentChapterIndex = data.currentChapterIndex;

            // Tạo RuntimeQuests từ SO
            if (savedChapter.defaultQuests != null)
                RuntimeQuests = Instantiate(savedChapter.defaultQuests);

            // Restore quest status từ save (full objects, không phải minimal copies)
            if (data.quests != null && RuntimeQuests != null && data.quests.Count > 0)
                RuntimeQuests.quests = data.quests;

            // Notify systems (QuestUI InitialDisplay, DialogueSystem, v.v.)
            EventManager.Notify(GameEvents.ChapterManager.OnChapterLoaded, CurrentChapterId);
        }


        /// <summary>
        /// Hàm này sẽ xử lý dữ liệu khi một chapter kết thúc
        /// </summary>
        /// <param name="loadNextImmediately">
        /// - true: Chơi tiếp luôn (kiểu mạch truyện liền mạch)
        /// - false: Về Menu (để chọn màn khác)
        /// </param>
        public void FinishCurrentChapter(bool loadNextImmediately = true)
        {
            // Tính toán Index tiếp theo
            int nextChapterIndex = _currentChapterIndex + 1;

            // Gọi SaveLoadManager để chốt sổ (Tạo file checkpoint)
            // Dù đi tiếp hay về menu thì việc "Lưu đã xong màn này" là bắt buộc
            SaveLoadManager.Instance.CompleteChapter(nextChapterIndex);

            // Cập nhật biến index nội bộ để game hiểu là đã sang chương mới
            _currentChapterIndex = nextChapterIndex;

            // Trường hợp 1: Hết game (Không còn chapter nào nữa)
            if (nextChapterIndex >= _chapterProgression.Count)
            {
                Debug.Log("🎉 CHÚC MỪNG! ĐÃ PHÁ ĐẢO GAME!");
                // Luôn về Menu hoặc màn Credit
                SceneTransitionManager.Instance.TransitionTo("MainMenu", "None");
                return;
            }

            ChapterDataSO nextChapterData = _chapterProgression[nextChapterIndex];

            if (loadNextImmediately)
            {
                // TRƯỜNG HỢP A: Chuyển luôn sang Chapter kế tiếp (Seamless)
                Debug.Log("⏩ Đang chuyển thẳng sang Chapter tiếp theo...");
                // if (SceneTransitionManager.Instance != null)
                // {
                //     SceneTransitionManager.Instance.TransitionTo(nextChapterData.sceneName, nextChapterData.startSpawnID);
                // }
            }
            else
            {
                // TRƯỜNG HỢP B: Về Menu (Để người chơi tự chọn chapter mới mở trong danh sách)
                Debug.Log("🏠 Đã xong màn. Đang quay về Menu...");
                if (SceneTransitionManager.Instance != null)
                {
                    // Giả sử scene menu của bạn tên là "MainMenu"
                    // SpawnID để null hoặc "MenuStart" tùy bạn setup
                    SceneTransitionManager.Instance.TransitionTo("MainMenu", "");
                }
            }
        }
    }
}


