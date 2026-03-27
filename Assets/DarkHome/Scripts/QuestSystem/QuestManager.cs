using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DarkHome
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public List<Quest> GetAllQuests() => GetRuntimeQuestData().quests;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad sẽ do PersistentManagers quản lý
        }

        private void OnEnable()
        {
            EventManager.AddObserver<QuestEventData>(GameEvents.Quest.OnQuestProgress, OnQuestProgressReceived);
            EventManager.AddObserver<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<QuestEventData>(GameEvents.Quest.OnQuestProgress, OnQuestProgressReceived);
            EventManager.RemoveListener<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        private void OnFlagChanged(FlagData changedFlag)
        {
            foreach (var quest in GetActiveQuests())
                CheckQuestCompletion(quest);
        }

        private void OnQuestProgressReceived(QuestEventData data)
        {
            // Duyệt qua tất cả Quest đang chạy
            foreach (var quest in GetAllQuests())
            {
                if (quest.Status != EQuestStatus.Active) continue; // Chỉ check quest đang làm

                // Duyệt qua từng mục tiêu con (Objective)
                foreach (var obj in quest.Objectives)
                {
                    // SO SÁNH:
                    // - Loại hành động có khớp không? (Cùng là Collect?)
                    // - ID có khớp không? (Cùng là 'Quest_Milk'?)
                    if (obj.Type == data.Type && obj.TargetID == data.TargetID)
                    {
                        // Cộng dồn số lượng
                        obj.CurrentAmount += data.Amount;

                        string objDesc = LocalizationManager.Instance.GetText(obj.DescriptionKey);
                        // Debug.Log($"✅ [Quest] Cập nhật: {objDesc} ({obj.CurrentAmount}/{obj.RequiredAmount})");

                        // Kiểm tra xem xong Quest chưa (Hàm cũ của bạn)
                        CheckQuestCompletion(quest);
                    }
                }
            }
        }

        private void CheckQuestCompletion(Quest quest)
        {
            // Notify UI cho từng objective vừa complete
            foreach (var obj in quest.Objectives)
            {
                bool done = obj.IsCompleted;
                Debug.Log($"[Quest] Obj: {obj.DescriptionKey} | IsCompleted: {done}");
                if (obj.CompletionFlags != null)
                    foreach (var f in obj.CompletionFlags)
                        Debug.Log($"Flag: {f.FlagID} (Scope:{f.Scope}) | HasFlag: {FlagManager.Instance.HasFlag(f)}");

                if (done)
                {
                    // Fire OnCompleteTriggers (VD: grant C1_QUEST_DAY1_EXPLORED khi explore xong)
                    if (obj.OnCompleteTriggers != null)
                        foreach (var t in obj.OnCompleteTriggers)
                            EventTriggerManager.Instance.ActiveEvent(t);

                    EventManager.Notify(GameEvents.Objective.OnObjectiveStatusChanged, obj);
                }
            }

            bool isAllDone = quest.Objectives.All(obj => obj.IsCompleted);

            if (isAllDone)
                CompleteQuest(quest.Id);
            else
                EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, quest);
        }

        private QuestDataSO GetRuntimeQuestData()
        {
            if (ChapterManager.Instance != null)
            {
                return ChapterManager.Instance.RuntimeQuests;
            }
            return null;
        }

        public Quest GetQuestById(string id)
        {
            var data = GetRuntimeQuestData();
            if (data == null)
            {
                // Nếu vào đây nghĩa là GameManager hoặc ChapterManager cấu hình sai
                Debug.LogError("QuestManager: RuntimeQuests bị Null! Game chưa khởi tạo đúng cách.");
                return null;
            }
            return data.quests.FirstOrDefault(q => q.Id == id);
        }


        public List<Quest> GetActiveQuests()
        {
            var questData = GetRuntimeQuestData();
            if (questData == null) return new List<Quest>();

            // Dùng LINQ để lọc trực tiếp từ "Nguồn sự thật"
            return questData.quests.Where(q => q.Status == EQuestStatus.Active).ToList();
        }

        public List<Quest> GetCompletedQuests()
        {
            var questData = GetRuntimeQuestData();
            if (questData == null) return new List<Quest>();

            return questData.quests.Where(q => q.Status == EQuestStatus.Completed).ToList();
        }

        public void HandleQuestTrigger(string id)
        {
            // Debug.Log($"QuestManager: HandleQuestTrigger: {id}");
            if (string.IsNullOrEmpty(id)) return;

            Quest q = GetQuestById(id); // Gọi hàm trên để lấy quest

            if (q == null)
            {
                Debug.LogError($"QuestManager: Không tìm thấy Quest ID '{id}' trong Chapter hiện tại!");
                return;
            }

            string questName = LocalizationManager.Instance.GetText(q.QuestNameKey);
            // Debug.Log($"Tìm thấy Quest: {questName} | Status: {q.Status}");

            // Logic xử lý trạng thái
            switch (q.Status)
            {
                case EQuestStatus.Inactive:
                    ActivateQuest(id);
                    break;
                case EQuestStatus.Active:
                    CompleteQuest(id);
                    break;
            }
        }


        // Bạn có thể tạo thêm các hàm tương tự cho Skipped, Failed...

        public void ActivateQuest(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            Quest q = GetQuestById(id);
            if (q == null) return;

            if (q.Status == EQuestStatus.Inactive && FlagManager.Instance.HasAllFlags(q.RequiredFlags))
            {
                q.Status = EQuestStatus.Active;
                EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, q);
                string questName = LocalizationManager.Instance.GetText(q.QuestNameKey);
                // Debug.Log($"Quest Activated: {questName}");

                // Check objectives ngay sau khi activate — tránh race condition
                // (VD: OBJ_DAY1_WAKE dùng cùng flag C1_PROGRESS_DAY1_STARTED)
                CheckQuestCompletion(q);
            }
        }

        public void CompleteQuest(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            Quest q = GetQuestById(id);
            if (q == null) return;

            // Ví dụ: Kiểm tra xem tất cả objective đã xong chưa (logic này có thể nằm ở QuestObjectiveHandler)
            bool allObjectivesDone = QuestObjectiveHandler.Instance.AreAllObjectivesCompleteForQuest(q.Id);

            if (q.Status == EQuestStatus.Active && allObjectivesDone)
            {
                q.Status = EQuestStatus.Completed;

                // Fire quest completion triggers (replaces GrantedFlags)
                if (q.OnCompleteTriggers != null && q.OnCompleteTriggers.Count > 0)
                {
                    foreach (var trigger in q.OnCompleteTriggers)
                    {
                        EventTriggerManager.Instance.ActiveEvent(trigger);
                        // Debug.Log($"[Quest Complete] {q.Id} triggered: {trigger.FlagID}");
                    }
                }

                EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, q);
                string questName = LocalizationManager.Instance.GetText(q.QuestNameKey);
                // Debug.Log($"Quest Completed: {questName}");

                // Logic save game có thể lắng nghe event OnQuestStatusChanged để quyết định
                // if (q.Type == EQuestType.Main) SaveLoadManager.Instance.SaveGame();
            }
        }

        public void SkipQuest(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            Quest q = GetQuestById(id);
            if (q == null) return;

            if (q.Status == EQuestStatus.Active || q.Status == EQuestStatus.Inactive)
            {
                q.Status = EQuestStatus.Skipped;
                EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, q);
            }
        }

        public void RefreshQuestUI()
        {
            if (GetRuntimeQuestData() == null)
            {
                Debug.LogWarning("[QuestManager] RefreshQuestUI: RuntimeQuests is null, skipping.");
                return;
            }

            var allQuests = GetAllQuests();
            if (allQuests == null) return;

            foreach (var q in allQuests)
            {
                if (q.Status == EQuestStatus.Active)
                    EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, q);
            }
        }
    }
}