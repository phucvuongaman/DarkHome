using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class DialogueFlowController : MonoBehaviour
    {
        private DialogueRepository _dialogueRepository;
        private DialogueNode _currentNode = null;


        private void OnEnable()
        {
            EventManager.AddObserver<string>(GameEvents.DiaLog.StartDialogueWithIdSpeaker, StartDialogueWithIdSpeaker);
            EventManager.AddObserver<string>(GameEvents.DiaLog.StartDialogueWithIdNode, StartDialogueWithIdNode);
            EventManager.AddObserver(GameEvents.DiaLog.NextDialogue, NextDialogue);
            EventManager.AddObserver<Choice>(GameEvents.DiaLog.OnChoiceSelected, OnChoiceSelected);

            EventManager.AddObserver<string>(GameEvents.ChapterManager.OnChapterLoaded, HandleChapterLoaded);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.DiaLog.StartDialogueWithIdSpeaker, StartDialogueWithIdSpeaker);
            EventManager.RemoveListener<string>(GameEvents.DiaLog.StartDialogueWithIdNode, StartDialogueWithIdNode);
            EventManager.RemoveListener(GameEvents.DiaLog.NextDialogue, NextDialogue);
            EventManager.RemoveListener<Choice>(GameEvents.DiaLog.OnChoiceSelected, OnChoiceSelected);

            EventManager.RemoveListener<string>(GameEvents.ChapterManager.OnChapterLoaded, HandleChapterLoaded);
        }


        private void HandleChapterLoaded(string chapterId)
        {
            Debug.Log($"[DialogueFlowController] Nhận lệnh tải dialogue cho chapter: {chapterId}");

            // Load DialogueDataSO từ 1 path duy nhất
            // Localization được xử lý bởi LocalizationManager khi resolve keys
            // Giống như Quest system - 1 file SO, nhiều ngôn ngữ!
            string path = $"{chapterId}/Dialogues";

            var allDialoguesForChapter = new List<DialogueDataSO>(Resources.LoadAll<DialogueDataSO>(path));

            // Tạo một new đống này vào _dialogueRepository
            _dialogueRepository = new DialogueRepository(allDialoguesForChapter);

            // Debug.Log($"✅ Đã tải {_dialogueRepository.GetDataSourceCount()} DialogueDataSO từ: Resources/{path}");
        }


        // Hàm này sẽ được phát tín hiệu khi tôi ấn E interact vào một NPC để nói chuyện
        private void StartDialogueWithIdSpeaker(string idSpeaker)
        {
            // Không cần tạo repository mới nữa, chỉ cần dùng cái đã có
            if (_dialogueRepository == null)
            {
                Debug.LogError("Dialogue Repository chưa được khởi tạo!");
                return;
            }

            // Lấy node nào đó mà đủ flag và đủ độ ưu tiên
            // Logic khó nhớ, qua DialogueRepository để nhớ lại
            _currentNode = _dialogueRepository.GetFirstTalkableOrDefault(idSpeaker);

            // nếu node vừa tìm được không null thì 
            if (_currentNode != null)
            {
                // ...đánh thêm tracking là đã nói
                // Mark as talked (but only if non-repeatable or StartNode)
                if (_currentNode.IsStartNode || !_currentNode.IsRepeatable)
                {
                    DialogueTrackingState.Instance.MarkNodeAsTalked(_currentNode.NodeId);
                }
                // ...gọi event hiển thị node
                EventManager.Notify(GameEvents.DiaLog.ReadDialogue, _currentNode);
            }
            else
            {
                // Nếu không có node thoại nào, có thể thử hiển thị Choice (nếu có)
                EvaluateAndShowChoices(null); // mà khả năng để null là nó end luôn
            }
        }

        private void StartDialogueWithIdNode(string idNode)
        {
            // if (_dialogueRepository == null) return;

            // _currentNode = _dialogueRepository.GetNodeById(idNode);

            // if (_currentNode != null)
            // {
            //     DialogueTrackingState.Instance.MarkNodeAsTalked(_currentNode.NodeId);
            //     EventManager.Notify(GameEvents.DiaLog.ReadDialogue, _currentNode);
            // }
            // else
            // {
            //     // Nếu không có node thoại nào, có thể thử hiển thị Choice (nếu có)
            //     EvaluateAndShowChoices(null); // mà khả năng để null là nó end luôn
            // }


            // Debug.Log($"🔍 [DEBUG] Nhận lệnh mở Node ID: {idNode}");

            if (_dialogueRepository == null)
            {
                Debug.LogError("🛑 [LỖI] Repository là NULL! Chưa load data chapter.");
                return;
            }

            _currentNode = _dialogueRepository.GetNodeById(idNode);

            if (_currentNode != null)
            {
                // Debug.Log("✅ [DEBUG] Tìm thấy Node! Đang gửi lệnh hiển thị.");
                // Mark as talked (but only if non-repeatable or StartNode)
                if (_currentNode.IsStartNode || !_currentNode.IsRepeatable)
                {
                    DialogueTrackingState.Instance.MarkNodeAsTalked(_currentNode.NodeId);
                }
                EventManager.Notify(GameEvents.DiaLog.ReadDialogue, _currentNode);
            }
            else
            {
                Debug.LogError($"❌ [LỖI] Repository đã load nhưng KHÔNG TÌM THẤY Node ID: {idNode}");
                EvaluateAndShowChoices(null); // mà khả năng để null là nó end luôn
            }
        }

        public void NextDialogue()
        {
            if (_currentNode == null)
            {
                EventManager.Notify(GameEvents.DiaLog.EndDialogue);
                return;
            }

            DialogueNode nextNode = _dialogueRepository.GetNodeById(_currentNode.NextId);
            if (nextNode != null)
            {
                _currentNode = nextNode;
                // Mark as talked (but only if non-repeatable or StartNode)
                if (_currentNode.IsStartNode || !_currentNode.IsRepeatable)
                {
                    DialogueTrackingState.Instance.MarkNodeAsTalked(_currentNode.NodeId);
                }
                EventManager.Notify(GameEvents.DiaLog.ReadDialogue, _currentNode);
            }
            else
            {
                EvaluateAndShowChoices(_currentNode);
            }
        }


        // Dán hàm này vào để thay thế hàm EvaluateAndShowChoices cũ
        private void EvaluateAndShowChoices(DialogueNode node)
        {
            // Logic mới: Đơn giản hơn rất nhiều!
            if (node == null || node.Choices == null || node.Choices.Count == 0)
            {
                EventManager.Notify(GameEvents.DiaLog.EndDialogue);
                return;
            }

            // Lọc ra các choice hợp lệ dựa trên FlagManager
            List<Choice> validChoices = new List<Choice>();
            foreach (var choice in node.Choices)
            {
                // 🔹 1. Kiểm tra cờ YÊU CẦU (phải có đủ TẤT CẢ)
                bool hasRequired = FlagManager.Instance.HasAllFlags(choice.RequiredFlags);

                // 🔹 2. Kiểm tra cờ ẨN (chỉ cần có BẤT KỲ 1 cờ là đủ)
                // (May quá, tôi đã code sẵn hàm `HasAnyFlag` trong FlagManager!)
                bool hasHiding = FlagManager.Instance.HasAnyFlag(choice.HidingFlags);

                // 🔹 3. Quyết định cuối cùng:
                if (!choice.IsHidden &&   // Không bị ẩn vĩnh viễn
                    hasRequired &&        // Đủ cờ để thấy
                    !hasHiding)           // KHÔNG có cờ nào để ẩn
                {
                    validChoices.Add(choice);
                }
            }

            if (validChoices.Count > 0)
            {
                // Gửi các lựa chọn hợp lệ đi để hiển thị
                UIManager.Instance.ShowChoices(validChoices);
            }
            else
            {
                // Nếu không có lựa chọn nào hợp lệ, kết thúc hội thoại
                EventManager.Notify(GameEvents.DiaLog.EndDialogue);
            }
        }

        private void OnChoiceSelected(Choice choice)
        {
            // // 1. Kích hoạt quest (nếu có)
            // if (!string.IsNullOrEmpty(choice.ActivateQuestID))
            // {
            //     // Giao việc cho QuestManager, nó sẽ tự kiểm tra điều kiện
            //     QuestManager.Instance.ActivateQuest(choice.ActivateQuestID);
            // }

            // // 2. Hoàn thành quest (nếu có)
            // if (!string.IsNullOrEmpty(choice.CompleteQuestID))
            // {
            //     // Giao việc cho QuestManager
            //     QuestManager.Instance.CompleteQuest(choice.CompleteQuestID);
            // }

            // 3. ✅ Fire event triggers (Quest activation, cutscenes, flag tracking, etc.)
            if (choice.OnSelectTriggers != null && choice.OnSelectTriggers.Count > 0)
            {
                foreach (var trigger in choice.OnSelectTriggers)
                {
                    EventTriggerManager.Instance.ActiveEvent(trigger);
                    Debug.Log($"🎬 [Choice] Triggered event: {trigger.FlagID}");
                }
            }

            // 4. Ra lệnh cho UIManager ẩn Choice UI đi (thay vì phát event)
            UIManager.Instance.HideChoices();

            // 5. Chuyển đến node tiếp theo hoặc kết thúc
            if (!string.IsNullOrEmpty(choice.NextNodeID))
            {
                _currentNode = _dialogueRepository.GetNodeById(choice.NextNodeID);
                if (_currentNode != null)
                {
                    UIManager.Instance.ShowDialogue(_currentNode);
                }
                else // Nếu không tìm thấy node tiếp theo, kết thúc hội thoại
                {
                    _currentNode = null;  // CRITICAL: Reset currentNode
                    EventManager.Notify(GameEvents.DiaLog.EndDialogue);  // CRITICAL: Fire event cho NPC
                    UIManager.Instance.HideDialogue();
                }
            }
            else
            {
                _currentNode = null;  // CRITICAL: Reset currentNode
                EventManager.Notify(GameEvents.DiaLog.EndDialogue);  // CRITICAL: Fire event cho NPC
                UIManager.Instance.HideDialogue();
            }
        }
    }
}