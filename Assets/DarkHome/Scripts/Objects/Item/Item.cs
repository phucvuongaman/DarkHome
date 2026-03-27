using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [RequireComponent(typeof(OutLineController))]
    [Serializable]
    public class Item : BaseObject
    {
        public ItemDataSO itemData;
        public override string Id => itemData != null ? itemData.itemID : base.Id;
        // InteractableName: Use cached value from BaseObject.LoadFromSO() → No override needed!

        public override InteractableType InteractType => InteractableType.Item;
        protected OutLineController outLine;

        protected override void Awake()
        {
            base.Awake();
            outLine = GetComponent<OutLineController>();
        }

        /// <summary>
        /// Override để load Item-specific data từ ItemDataSO.
        /// Lưu ý: Item.cs có cả itemData field (Inspector reference) VÀ _objectData (SO reference).
        /// Nếu _objectData assigned → dùng nó. Nếu không → fallback itemData.
        /// </summary>
        protected override void LoadFromSO()
        {
            base.LoadFromSO();  // Load common fields first

            // Type-check và sync itemData reference
            if (_objectData is ItemDataSO itemDataSO)
            {
                itemData = itemDataSO;  // Sync itemData reference với _objectData
                // Debug.Log($" [Item] {name}: Loaded ItemDataSO - ID: {itemData.itemID}, Type: {itemData.itemType}");
            }
        }

        // private void Start()
        // {
        //     ObjectManager.Instance.Register(this);
        // }
        public override void OnFocus()
        {
            outLine?.EnableOutline();
        }
        public override void OnLoseFocus()
        {
            outLine?.DisableOutline();
        }


        public override void OnInteractPress(Interactor interactor)
        {
            base.OnInteractPress(interactor);

            // QuestObjectiveHandler.Instance.SetQuestObjectiveComplete(this.Id);
            QuestObjectiveHandler.Instance.SetQuestObjectiveComplete(itemData.itemID, itemData.questKey);
            EventManager.Notify(GameEvents.Object.OnItemCollected, itemData);

            var data = new QuestEventData
            {
                Type = EQuestObjectiveType.Collect,
                TargetID = itemData.itemID,
                Amount = 1
            };

            // Gọi hàm Notify<T> có sẵn trong EventManager
            EventManager.Notify<QuestEventData>(GameEvents.Quest.OnQuestProgress, data);

            // ✨ Apply item effects (Sanity/Health) via Interactor
            ApplyItemEffects(interactor);

            // 🗑️ Destroy item after pickup (if configured)
            if (itemData != null && itemData.destroyOnPickup)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Apply stat changes (Sanity/Health) through Interactor -> PlayerContext chain.
        /// No Find...() calls, uses parameter properly.
        /// </summary>
        private void ApplyItemEffects(Interactor interactor)
        {
            if (itemData.onPickupEffects == null || itemData.onPickupEffects.Length == 0) return;

            // Get PlayerStats via Interactor (attached to same GameObject as PlayerContext)
            PlayerContext playerContext = interactor.GetComponent<PlayerContext>();
            if (playerContext == null || playerContext.Stats == null)
            {
                Debug.LogWarning($"[Item] {itemData.itemName}: Không tìm thấy PlayerContext/Stats!", this);
                return;
            }

            PlayerStats stats = playerContext.Stats;

            foreach (var effect in itemData.onPickupEffects)
            {
                switch (effect.effectType)
                {
                    case EItemEffectType.ModifySanity:
                        stats.ApplySanityChange(effect.value);
                        Debug.Log($"💊 [{itemData.itemName}] Sanity {effect.value:+0;-0}");
                        break;

                    case EItemEffectType.ModifyHealth:
                        stats.ApplyDamage(-effect.value); // Negative damage = Heal
                        Debug.Log($"💊 [{itemData.itemName}] Health {effect.value:+0;-0}");
                        break;
                }
            }
        }
    }
}