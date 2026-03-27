using UnityEngine;
using TMPro;

namespace DarkHome
{
    public class QuestDisplayItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _questNameText;
        [SerializeField] private TextMeshProUGUI _questDescriptionText;
        [SerializeField] private Transform _objectivesContainer;
        [SerializeField] private GameObject _objectiveDisplayPrefab; // Kéo ObjectiveDisplay_Prefab vào đây

        public void Initialize(Quest quest)
        {
            // Use LocalizationManager for text display
            _questNameText.text = LocalizationManager.Instance.GetText(quest.QuestNameKey);
            _questDescriptionText.text = LocalizationManager.Instance.GetText(quest.DescriptionKey);

            // Dọn dẹp objective cũ (nếu có)
            foreach (Transform child in _objectivesContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo và hiển thị các objective mới
            if (quest.Objectives != null)
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    var objective = quest.Objectives[i];

                    // CHECK DisplayMode LOGIC
                    if (!ShouldShowObjective(quest, objective, i))
                    {
                        continue; // Skip this objective
                    }

                    GameObject newObjUI = Instantiate(_objectiveDisplayPrefab, _objectivesContainer);
                    newObjUI.GetComponent<QuestObjectiveInfo>().Initialize(objective);
                }
            }
        }

        /// <summary>
        /// Checks if objective should be shown based on Quest DisplayMode
        /// </summary>
        private bool ShouldShowObjective(Quest quest, QuestObjective objective, int objectiveIndex)
        {
            switch (quest.DisplayMode)
            {
                case EQuestDisplayMode.Sequential:
                    // Single-flag hoàn thành → ẩn luôn, next objective hiện
                    // Multi-flag hoàn thành (VD: Explore 4 đồ) → show gạch ngang
                    bool isMultiFlag = objective.CompletionFlags != null && objective.CompletionFlags.Count > 1;
                    if (objective.IsCompleted && !isMultiFlag) return false;

                    // Kiểm tra tất cả objectives TRƯỚC có done chưa
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
                        return true; // Không có điều kiện → Hiện
                    }
                    return FlagManager.Instance.HasAllFlags(objective.RequiredFlagsToAppear);

                default:
                    return true; // Fallback: Hiện tất cả
            }
        }
    }
}