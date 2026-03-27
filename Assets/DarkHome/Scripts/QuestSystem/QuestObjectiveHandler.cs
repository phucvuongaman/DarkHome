using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DarkHome
{
    public class QuestObjectiveHandler : MonoBehaviour
    {
        public static QuestObjectiveHandler Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetQuestObjectiveComplete(string targetId, string questKey = null)
        {
            if (string.IsNullOrEmpty(targetId)) return;

            var activeQuests = QuestManager.Instance.GetActiveQuests();
            if (activeQuests == null || activeQuests.Count == 0) return;

            Quest ownerQuest = null;
            QuestObjective objectiveToComplete = null;

            foreach (var q in activeQuests)
            {
                // LOGIC NÂNG CẤP Ở ĐÂY
                // Tìm mục tiêu chưa xong (IsCompleted == false)
                // VÀ thỏa mãn 1 trong 2 điều kiện:
                // - Trùng ID đích danh (nhặt đúng món đó)
                // - HOẶC Trùng QuestKey (nhặt món thuộc nhóm đó)
                var obj = q.Objectives.FirstOrDefault(o =>
                    !o.IsCompleted &&
                    (
                        o.TargetID == targetId ||
                        (!string.IsNullOrEmpty(questKey) && o.TargetID == questKey)
                    )
                );

                if (obj != null)
                {
                    ownerQuest = q;
                    objectiveToComplete = obj;
                    break;
                }
            }

            // --- ĐOẠN DƯỚI NÀY GIỮ NGUYÊN ---
            if (objectiveToComplete != null)
            {
                objectiveToComplete.CurrentAmount++;
                // Debug.Log($"[Quest] Nhặt: {targetId} (Key: {questKey}) -> {objectiveToComplete.CurrentAmount}/{objectiveToComplete.RequiredAmount}");

                if (objectiveToComplete.IsCompleted)
                {
                    // CompletionFlags for logic checking
                    if (objectiveToComplete.CompletionFlags != null)
                        FlagManager.Instance.AddFlags(objectiveToComplete.CompletionFlags);

                    // Fire objective completion triggers
                    if (objectiveToComplete.OnCompleteTriggers != null && objectiveToComplete.OnCompleteTriggers.Count > 0)
                    {
                        foreach (var trigger in objectiveToComplete.OnCompleteTriggers)
                        {
                            EventTriggerManager.Instance.ActiveEvent(trigger);
                            // Debug.Log($"[Objective Complete] {objectiveToComplete.DescriptionKey} triggered: {trigger.FlagID}");
                        }
                    }

                    EventManager.Notify(GameEvents.Objective.OnObjectiveStatusChanged, objectiveToComplete);

                    // Check if ALL objectives complete before completing quest
                    if (ownerQuest != null)
                    {
                        if (ownerQuest.Objectives.All(obj => obj.IsCompleted))
                        {
                            // All objectives complete → Complete quest
                            QuestManager.Instance.CompleteQuest(ownerQuest.Id);
                        }
                        else
                        {
                            // Some objectives remaining → Just notify UI to update
                            EventManager.Notify(GameEvents.Quest.OnQuestStatusChanged, ownerQuest);
                            // Debug.Log($"[Quest] {ownerQuest.Id}: Objective complete, but quest still active.");
                        }
                    }
                }
            }
        }

        public bool AreAllObjectivesCompleteForQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return false;

            Quest quest = QuestManager.Instance.GetQuestById(questId);
            if (quest == null || quest.Objectives == null || quest.Objectives.Count == 0)
            {
                return true;
            }

            // Dùng Property IsCompleted
            return quest.Objectives.All(o => o.IsCompleted);
        }

        private void OnEnable()
        {
            EventManager.AddObserver<PuzzleDataSO>(GameEvents.Puzzle.OnPuzzleSolved, HandlePuzzleSolved);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<PuzzleDataSO>(GameEvents.Puzzle.OnPuzzleSolved, HandlePuzzleSolved);
        }

        private void HandlePuzzleSolved(PuzzleDataSO solvedPuzzleData)
        {
            SetQuestObjectiveComplete(solvedPuzzleData.Id);
        }
    }
}