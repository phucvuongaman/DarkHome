using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public abstract class PuzzleBase : BaseObject
    {
        // public override InteractableType InteractType { get; set; }
        [SerializeField] protected PuzzleDataSO puzzleData;
        public abstract bool CheckSolved(); // Custom logic ở từng puzzle con

        public virtual void TrySolve()
        {
            if (!CheckSolved()) return;

            Debug.Log($"Puzzle {Id} solved!");

            // --- THAY ĐỔI CHÍNH ---
            // Chỉ cần phát đi một tín hiệu duy nhất, gửi kèm dữ liệu của puzzle
            if (puzzleData != null)
            {
                FlagManager.Instance.AddFlags(puzzleData.GrantedFlags);
                EventManager.Notify(GameEvents.Puzzle.OnPuzzleSolved, puzzleData);
            }

            // //  Gắn flag
            // FlagManager.Instance.AddFlags(puzzleData.GrantedFlags);

            // // Thông báo hoàn thành objective (nếu gắn quest)
            // EventManager.Notify(GameEvents.Quest.SetQuestObjectiveComplete, Id);

            // // (Optional) Kích hoạt quest mới
            // if (!string.IsNullOrEmpty(activateQuestID))
            //     QuestManager.Instance.ActivateQuest(activateQuestID);

            // if (!string.IsNullOrEmpty(completeQuestID))
            //     QuestManager.Instance.CompleteQuest(completeQuestID);
        }

        public override void OnInteractPress(Interactor interactor) { }


    }

}