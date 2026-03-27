using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkHome
{
    // Kế thừa BaseObject để có sẵn RequiredFlags, InteractableName...
    public class Bed : BaseObject
    {
        [Header("Loop Configuration")]
        [Tooltip("Danh sách ID các Chapter cho phép cơ chế Ngủ/Loop (VD: Chapter1)")]
        [SerializeField] private List<string> _loopableChapterIDs;

        [Tooltip("Điểm Spawn khi thức dậy (Thường là cạnh giường)")]
        [SerializeField] private string _wakeUpSpawnID = "WakeUpPoint";

        [Header("Narrative")]
        [Tooltip("Câu thoại khi Player cố ngủ ở các chương sau (Tuyến tính)")]
        [SerializeField] private string _cantSleepDialogueID;

        // Override lại InteractType (Cần thiết cho BaseObject)
        public override InteractableType InteractType => InteractableType.Bed;

        /// <summary>
        /// Override để load Bed-specific data từ BedDataSO.
        /// </summary>
        protected override void LoadFromSO()
        {
            base.LoadFromSO();  // Load common fields first

            // Type-check và load Bed-specific fields
            if (_objectData is BedDataSO bedData)
            {
                _loopableChapterIDs = bedData.loopableChapterIDs;
                _wakeUpSpawnID = bedData.wakeUpSpawnID;
                _cantSleepDialogueID = bedData.cantSleepDialogueID;

                Debug.Log($"✅ [Bed] {name}: Loaded BedDataSO - Loopable Chapters: {_loopableChapterIDs.Count}, WakeUpSpawn: {_wakeUpSpawnID}");
            }
        }

        // BaseObject yêu cầu hàm này, ta để trống hoặc thêm logic active/inactive vật lý nếu muốn
        protected override void OnInteractableStateChanged(bool canInteract)
        {
            // Ví dụ: Nếu không đủ Flag để ngủ, có thể đổi màu giường hoặc tắt highlight
        }

        public override void OnInteractPress(Interactor interactor)
        {
            base.OnInteractPress(interactor);

            // Kiểm tra Flag (Logic có sẵn của BaseObject đã check rồi, 
            // nhưng check lại ở đây cũng không thừa nếu muốn logic custom)
            if (!FlagManager.Instance.HasAllFlags(RequiredFlags))
            {
                // Nếu thiếu Flag (ví dụ chưa khóa cửa), hiện thông báo
                Debug.Log("Chưa đủ điều kiện để ngủ!");
                // EventManager.Notify(GameEvents.DiaLog.StartDialogueWithIdNode, "Dialogue_NotTiredYet");
                return;
            }

            string currentChapter = ChapterManager.Instance.CurrentChapterId;

            // Kiểm tra Chapter
            if (_loopableChapterIDs.Contains(currentChapter))
            {
                // Set completion flag for current day BEFORE sleep
                if (SaveLoadManager.Instance != null)
                {
                    int currentDay = SaveLoadManager.Instance.GetCurrentDay();
                    string completionFlag = $"C1_PROGRESS_DAY{currentDay}_COMPLETE";
                    FlagManager.Instance.AddFlag(new FlagData(completionFlag, EFlagScope.Local));
                    // Debug.Log($"[Bed] Set {completionFlag}");
                }

                StartCoroutine(SleepSequence());
            }
            else
            {
                // Debug.Log($" Chapter '{currentChapter}' không cho phép ngủ.");
                if (!string.IsNullOrEmpty(_cantSleepDialogueID))
                {
                    EventManager.Notify(GameEvents.DiaLog.StartDialogueWithIdNode, _cantSleepDialogueID);
                }
            }
        }

        private IEnumerator SleepSequence()
        {
            // Debug.Log(" Đang ngủ... Chuyển sang ngày mới.");

            // Tăng ngày & Lưu Game
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveSceneToMemory();
                SaveLoadManager.Instance.IncreaseDayCount();
            }

            // Reload Scene
            string currentScene = SceneManager.GetActiveScene().name;

            SceneChangeData data = new SceneChangeData
            {
                SceneName = currentScene,
                TargetSpawnID = _wakeUpSpawnID
            };

            EventManager.Notify(GameEvents.SceneTransition.OnSceneChangeRequested, data);

            yield return null;
        }
    }
}