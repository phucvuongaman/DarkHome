using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace DarkHome
{
    [Serializable]
    public class TriggerArea : BaseObject
    {
        public override InteractableType InteractType => InteractableType.Trigger;

        [Header("Trigger Area Events")]
        [Tooltip("Các sự kiện sẽ kích hoạt khi Player đi vào")]
        [SerializeField] private List<FlagData> _onEnterTriggerIds;

        [Tooltip("Các sự kiện sẽ kích hoạt khi Player đi ra")]
        [SerializeField] private List<FlagData> _onExitTriggerIds;

        [Tooltip("Trigger này chỉ kích hoạt một lần duy nhất?")]
        [SerializeField] private bool _triggerOnce = true;

        private bool _hasBeenTriggered = false;

        /// <summary>
        /// Override để load TriggerArea-specific data từ AreaDataSO.
        /// </summary>
        protected override void LoadFromSO()
        {
            base.LoadFromSO();  // Load common fields first

            // Type-check và load Area-specific fields
            if (_objectData is AreaDataSO areaData)
            {
                _onEnterTriggerIds = areaData.onEnterTriggers;
                _onExitTriggerIds = areaData.onExitTriggers;
                _triggerOnce = areaData.triggerOnce;

                // Debug.Log($" [TriggerArea] {name}: Loaded AreaDataSO - OnEnter: {_onEnterTriggerIds?.Count ?? 0} triggers, TriggerOnce: {_triggerOnce}");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Kiểm tra xem đã kích hoạt chưa (nếu chỉ cho phép 1 lần)
                if (_triggerOnce && _hasBeenTriggered) return;

                // Gọi hàm tiện ích với đúng danh sách
                ActivateTriggers(_onEnterTriggerIds);

                _hasBeenTriggered = true;
                if (_triggerOnce)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Kiểm tra xem đã kích hoạt chưa (nếu chỉ cho phép 1 lần)
                if (_triggerOnce && _hasBeenTriggered) return;

                // Gọi hàm tiện ích với đúng danh sách
                ActivateTriggers(_onExitTriggerIds);

                _hasBeenTriggered = true;
                if (_triggerOnce)
                {
                    gameObject.SetActive(false);
                }
            }
        }


        // đè lại để đảm bảo không có gì xảy ra nếu ai đó vô tình gọi nó.
        public override void OnInteractPress(Interactor interactor) { }

    }
}