using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DarkHome
{
    public class QuestUI : MonoBehaviour
    {

        [Header("Quest Log Panels")]
        [Tooltip("Panel chính chứa toàn bộ giao diện Sổ tay Nhiệm-vụ")]
        [SerializeField] private GameObject _questLogPanel;

        [Header("Left Column - Quest List")]
        [Tooltip("Prefab của nút bấm để hiển thị tên nhiệm vụ")]
        [SerializeField] private GameObject _questButtonPrefab;
        [Tooltip("Transform 'Content' bên trong Scroll View để chứa các nút nhiệm vụ")]
        [SerializeField] private Transform _questListContent;

        [Header("Right Column - Quest Details")]
        [SerializeField] private GameObject _detailsPanel; // Kéo QuestDetails_Panel vào đây
        [SerializeField] private TextMeshProUGUI _questNameText;
        [SerializeField] private TextMeshProUGUI _questDescriptionText;
        [SerializeField] private Transform _objectivesListContent; // Kéo Objectives_List vào đây
        [SerializeField] private GameObject _objectiveDisplayPrefab; // Dùng lại prefab cũ

        [Header("HUD Quest on top-left screen")]
        // [SerializeField] private QuestDisplayItem _questDisplayPrefab; // Kéo QuestDisplay_Prefab vào đây
        [SerializeField] private Transform _mainQuestContainer; // Một slot riêng cho Main Quest
        [SerializeField] private Transform _sideQuestContainer; // Một Vertical Layout Group cho Side Quests

        // Dùng Dictionary để quản lý các UI đã tạo, key là Quest ID
        private Dictionary<string, QuestDisplayItem> _activeQuestUIs = new Dictionary<string, QuestDisplayItem>();

        private void OnEnable()
        {
            EventManager.AddObserver<Quest>(GameEvents.Quest.OnQuestStatusChanged, HandleQuestStatusChanged);
            EventManager.AddObserver<List<Quest>>(GameEvents.ChapterManager.OnChapterDataLoaded, InitialDisplay);
            // InitialDisplay(); // tamj thời comment để xử lý logic
            // InputManager.onJournalPressed += ToggleJournalPanel;
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Quest>(GameEvents.Quest.OnQuestStatusChanged, HandleQuestStatusChanged);
            EventManager.RemoveListener<List<Quest>>(GameEvents.ChapterManager.OnChapterDataLoaded, InitialDisplay);
            // InputManager.onJournalPressed -= ToggleJournalPanel;
        }

        #region Quest Journal
        // Hàm này sử lý bật tắt toggle  
        // public void ToggleJournalPanel()
        // {
        //     // Nyaf để trường hợp nếu tới hoặc trở về MainMenu
        //     if (GameManager.Instance.CurrentState == GameState.MainMenu)
        //     {
        //         _questLogPanel.SetActive(false);
        //         return;
        //     }

        //     // Kiểm tra xem Sổ tay có đang MỞ hay không
        //     bool isOpening = !_questLogPanel.activeSelf;


        //     // Bật/Tắt Panel Sổ tay
        //     _questLogPanel.SetActive(isOpening);

        //     // Ra lệnh cho GameManager thay đổi trạng thái
        //     //    (GameManager sẽ "ra lệnh" cho UIManager
        //     //    để dừng Time.timeScale và mở Cursor)
        //     GameManager.Instance.UpdateGameState(
        //         isOpening ? GameState.Paused : GameState.Gameplay);

        //     // (Tùy chọn) Cập nhật danh sách quest
        //     //    chỉ khi MỞ sổ
        //     if (isOpening)
        //     {
        //         UpdateQuestList();
        //     }
        // }

        // Hàm này sẽ được gọi để làm mới danh sách nhiệm vụ ở cột trái
        public void UpdateQuestList()
        {
            // Dọn dẹp
            foreach (Transform child in _questListContent) { Destroy(child.gameObject); }

            // Lấy 2 danh sách "Đã biết" (Known)
            var activeQuests = QuestManager.Instance.GetActiveQuests();
            var completedQuests = QuestManager.Instance.GetCompletedQuests();

            // Dùng LINQ (.Concat) để "nối" (join) chúng lại
            var allKnownQuests = activeQuests.Concat(completedQuests);

            // Tạo nút cho mỗi nhiệm vụ
            foreach (var quest in allKnownQuests)
            {
                GameObject buttonGO = Instantiate(_questButtonPrefab, _questListContent);

                // Get localized quest name
                string questName = LocalizationManager.Instance.GetText(quest.QuestNameKey);
                buttonGO.GetComponentInChildren<TextMeshProUGUI>().text = questName;

                // Quan trọng: Thêm listener để khi click vào nút, nó sẽ hiển thị chi tiết
                buttonGO.GetComponent<Button>().onClick.AddListener(() =>
                {
                    DisplayQuestDetails(quest);
                });
            }
        }

        // Hàm này sẽ hiển thị chi tiết của một nhiệm vụ được chọn ở cột phải
        public void DisplayQuestDetails(Quest quest)
        {
            if (quest == null) return;

            _detailsPanel.SetActive(true);

            // Use LocalizationManager for multi-language support
            _questNameText.text = LocalizationManager.Instance.GetText(quest.QuestNameKey);
            _questDescriptionText.text = LocalizationManager.Instance.GetText(quest.DescriptionKey);

            // Dọn dẹp danh sách mục tiêu cũ
            foreach (Transform child in _objectivesListContent)
            {
                Destroy(child.gameObject);
            }

            // Tạo các dòng mục tiêu mới
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];

                // CHECK DisplayMode LOGIC
                if (!ShouldShowObjective(quest, objective, i))
                {
                    continue; // Skip this objective
                }

                GameObject newObjUI = Instantiate(_objectiveDisplayPrefab, _objectivesListContent);
                newObjUI.GetComponent<QuestObjectiveInfo>().Initialize(objective);
            }
        }

        // Hiển thị các quest đã active sẵn khi game bắt đầu
        // private void InitialDisplay()
        // {
        //     var activeQuests = QuestManager.Instance.GetActiveQuests();
        //     foreach (var quest in activeQuests)
        //     {
        //         AddQuestToHUD(quest);
        //     }
        // }
        private void InitialDisplay(List<Quest> runtimeQuests)
        {
            var activeQuests = runtimeQuests.Where(q => q.Status == EQuestStatus.Active);
            foreach (var quest in activeQuests)
            {
                AddQuestToHUD(quest);
            }
        }
        #endregion

        #region Quest HUD
        private void HandleQuestStatusChanged(Quest quest)
        {
            if (quest.Status == EQuestStatus.Active)
            {
                if (!_activeQuestUIs.ContainsKey(quest.Id))
                {
                    AddQuestToHUD(quest);
                }
                else
                {
                    // Quest đang active, objectives thay đổi → refresh UI
                    _activeQuestUIs[quest.Id].Initialize(quest);
                }
            }
            else // Completed, Failed, Skipped
            {
                if (_activeQuestUIs.ContainsKey(quest.Id))
                {
                    RemoveQuestFromUI(quest);
                }
            }
        }


        private void AddQuestToHUD(Quest quest)
        {
            if (_mainQuestContainer.IsUnityNull() && _sideQuestContainer.IsUnityNull()) return;

            Transform parentContainer = (quest.Type == EQuestType.Main) ? _mainQuestContainer : _sideQuestContainer;

            // QuestDisplayItem newQuestUI = Instantiate(_questDisplayPrefab, parentContainer);

            // Code mới
            // Lấy một object từ pool với đúng key đã setup
            GameObject questGO = PoolManager.Instance.GetObjectFromPool("QuestDisplayItem");
            if (questGO == null) return; // Nếu có lỗi, dừng lại

            // Set vị trí cho nó
            questGO.transform.SetParent(parentContainer, false);

            // Lấy script và khởi tạo
            QuestDisplayItem newQuestUI = questGO.GetComponent<QuestDisplayItem>();
            newQuestUI.Initialize(quest); // Gửi dữ liệu quest cho UI item xử lý

            _activeQuestUIs.Add(quest.Id, newQuestUI);
        }


        // Đây là nơi "trả lại" cho PoolManager
        private void RemoveQuestFromUI(Quest quest)
        {
            if (_activeQuestUIs.TryGetValue(quest.Id, out QuestDisplayItem questUI))
            {
                _activeQuestUIs.Remove(quest.Id);
                // Destroy(questUI.gameObject);

                // Code mới
                // Trả object về lại pool để tái sử dụng
                PoolManager.Instance.ReturnToPool("QuestDisplayItem", questUI.gameObject);
            }
        }

        /// <summary>
        /// Checks if objective should be shown based on Quest DisplayMode
        /// Same logic as QuestDisplayItem for consistency
        /// </summary>
        private bool ShouldShowObjective(Quest quest, QuestObjective objective, int objectiveIndex)
        {
            switch (quest.DisplayMode)
            {
                case EQuestDisplayMode.Sequential:
                    // TUẦN TỰ: Chỉ hiện objective hiện tại + các objective đã xong
                    for (int i = 0; i < objectiveIndex; i++)
                    {
                        if (!quest.Objectives[i].IsCompleted) return false;
                    }
                    return true;

                case EQuestDisplayMode.Parallel:
                    // SONG SONG: Hiện tất cả objectives cùng lúc
                    return true;

                case EQuestDisplayMode.Custom:
                    // TÙY CHỈNH: Check RequiredFlagsToAppear
                    if (objective.RequiredFlagsToAppear == null || objective.RequiredFlagsToAppear.Count == 0)
                    {
                        return true;
                    }
                    return FlagManager.Instance.HasAllFlags(objective.RequiredFlagsToAppear);

                default:
                    return true;
            }
        }
        #endregion
    }
}