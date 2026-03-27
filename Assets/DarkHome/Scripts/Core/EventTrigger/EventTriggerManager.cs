using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace DarkHome
{
    public class EventTriggerManager : MonoBehaviour
    {
        public static EventTriggerManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
        }


        [SerializeField]
        private EventTriggerDataSO _eventTriggerData;


        public void ActiveEvent(FlagData changedFlag)
        {
            if (string.IsNullOrEmpty(changedFlag.FlagID)) return;

            // Lấy tất cả events có cùng ID
            var allMatches = _eventTriggerData.triggers
                .FindAll(e => e.Id == changedFlag.FlagID);

            if (allMatches.Count == 0)
            {
                Debug.LogWarning($"Không tìm thấy EventTrigger với ID: {changedFlag.FlagID}");
                return;
            }

            // Most-specific-wins: lấy event có nhiều RequiredFlags nhất mà vẫn pass hết
            // Nhiều flag hơn = cụ thể hơn = ưu tiên cao hơn (như CSS specificity)
            var trigger = allMatches
                .Where(t => FlagManager.Instance.HasAllFlags(t.RequiredFlags))
                .OrderByDescending(t => t.RequiredFlags != null ? t.RequiredFlags.Count : 0)
                .FirstOrDefault();

            if (trigger == null) return; // Không có event nào pass được RequiredFlags

            switch (trigger.Type)
            {
                case ETriggerType.Dialogue:
                    EventManager.Notify(GameEvents.DiaLog.StartDialogueWithIdNode, trigger.Data);

                    break;

                case ETriggerType.Cutscene:
                    // Placeholder: Fire StoryEvent → EndingScreenManager lắng nghe và hiển thị màn kết thúc
                    if (!string.IsNullOrEmpty(trigger.Data))
                    {
                        EventManager.Notify(GameEvents.Story.OnStoryEvent, trigger.Data);
                        Debug.Log($"🎬 [Cutscene] Firing ending: {trigger.Data}");
                    }
                    break;

                case ETriggerType.Quest:
                    // Ví dụ: dữ liệu có thể là "ACTIVATE:QuestID" hoặc "COMPLETE:QuestID"
                    QuestManager.Instance.HandleQuestTrigger(trigger.Data);
                    break;

                case ETriggerType.ObjectToggle:
                    if (trigger.TargetObject != null)
                        trigger.TargetObject.SetActive(!trigger.TargetObject.activeSelf);
                    break;

                case ETriggerType.Spawn:
                    if (trigger.TargetObject != null)
                        trigger.TargetObject.SetActive(true);
                    break;

                case ETriggerType.Despawn:
                    if (trigger.TargetObject != null)
                        trigger.TargetObject.SetActive(false);
                    break;

                case ETriggerType.Sound:
                    // Data: Tên file âm thanh trong Resources (Cách đơn giản)
                    // Hoặc cách tốt hơn: Dùng TargetObject chứa AudioSource
                    // Nhưng để nhanh gọn cho Vertical Slice, ta dùng AudioManager phát SFX

                    // Lưu ý: Để dùng cách này, cậu cần load AudioClip từ Resources hoặc gán sẵn.
                    // Cách Clean nhất hiện tại: TargetObject là nơi chứa AudioSource/AudioZone

                    if (trigger.TargetObject != null)
                    {
                        AudioSource source = trigger.TargetObject.GetComponent<AudioSource>();
                        if (source != null) source.Play();
                    }
                    // Hoặc nếu muốn dùng AudioManager phát tiếng click/thông báo chung:
                    // AudioManager.Instance.PlaySFX(Resources.Load<AudioClip>(trigger.Data));
                    break;

                case ETriggerType.Animation:
                    if (trigger.TargetObject != null)
                    {
                        Animator anim = trigger.TargetObject.GetComponent<Animator>();
                        if (anim != null)
                        {
                            // Data chứa tên Trigger Parameter (VD: "Open")
                            anim.SetTrigger(trigger.Data);
                        }
                    }
                    break;

                // case ETriggerType.Teleport:
                //     // Player.Instance.TeleportTo(trigger.targetPrefab.transform.position);
                //     break;

                case ETriggerType.FlagOnly:
                    // FlagManager.Instance.AddFlag(changedFlag);
                    break;
                case ETriggerType.SceneTransition:
                    // Cú pháp Data: "SceneName:SpawnID"
                    string[] parts = trigger.Data.Split(':');

                    if (parts.Length == 2)
                    {
                        string sceneName = parts[0].Trim();
                        string spawnID = parts[1].Trim();
                        SceneTransitionManager.Instance.TransitionTo(sceneName, spawnID);
                    }
                    else
                    {
                        Debug.LogError($"[EventTrigger] SceneTransition Data sai định dạng! Phải là 'SceneName:SpawnID'. (Data: {trigger.Data})");
                    }
                    break;
                case ETriggerType.MusicChange:
                    if (trigger.TargetObject != null)
                    {
                        // Lấy Clip từ AudioSource trên TargetObject (dùng làm vật chứa)
                        var source = trigger.TargetObject.GetComponent<AudioSource>();
                        if (source != null && source.clip != null)
                        {
                            // Gọi ChaseSystem (nếu dùng hệ thống này quản lý nhạc nền)
                            if (ChaseSystem.Instance != null) ChaseSystem.Instance.SetZoneMusic(source.clip);
                            // Hoặc gọi thẳng: AudioManager.Instance.PlayMusic(source.clip);
                        }
                    }
                    break;

                case ETriggerType.AmbienceChange:
                    if (trigger.TargetObject != null)
                    {
                        var source = trigger.TargetObject.GetComponent<AudioSource>();
                        if (source != null && source.clip != null)
                        {
                            AudioManager.Instance.PlayAmbience(source.clip);
                        }
                    }
                    break;
                case ETriggerType.SaveGame:
                    SaveLoadManager.Instance.SaveGame();
                    break;

                    // case ETriggerType.StoryEvent:
                    //     // Bắn pháo hiệu lên kênh Story
                    //     // trigger.Data chứa ID sự kiện (VD: "Event_Dinner")
                    //     if (!string.IsNullOrEmpty(trigger.Data))
                    //     {
                    //         EventManager.Notify(GameEvents.Story.OnStoryEvent, trigger.Data);
                    //         Debug.Log($"📣 EventManager: Đã phát lệnh Story [{trigger.Data}]");
                    //     }
                    //     else
                    //     {
                    //         Debug.LogWarning($"⚠️ StoryEvent ID trống! Kiểm tra lại Trigger {trigger.Id}");
                    //     }
                    //     break;
            }

            FlagManager.Instance.AddFlags(trigger.GrantedFlags);

        }

        private void OnEnable()
        {
            EventManager.AddObserver<FlagData>(GameEvents.Flag.OnFlagChanged, ActiveEvent);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<FlagData>(GameEvents.Flag.OnFlagChanged, ActiveEvent);
        }
    }
}