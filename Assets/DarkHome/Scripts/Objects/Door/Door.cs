using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [Serializable]
    public class Door : BaseObject
    {
        [SerializeField] private string _nextSceneName;
        [SerializeField] private string _targetSpawnID;
        public string Description;

        public override InteractableType InteractType => InteractableType.Door;

        /// <summary>
        /// Override để load Door-specific data từ DoorDataSO.
        /// </summary>
        protected override void LoadFromSO()
        {
            base.LoadFromSO();  // Load common fields first

            // Type-check và load Door-specific fields
            if (_objectData is DoorDataSO doorData)
            {
                _nextSceneName = doorData.nextSceneName;
                _targetSpawnID = doorData.targetSpawnID;
                Description = doorData.description;

                // Debug.Log($"[Door] {name}: Loaded DoorDataSO - NextScene: {_nextSceneName}, SpawnID: {_targetSpawnID}");
            }
        }

        protected override void OnInteractableStateChanged(bool canInteract)
        {
            // Để script bật tắt các thứ ở đây;
        }
        public override void OnInteractPress(Interactor interactor)
        {
            // Fire onInteractTriggers from DoorDataSO (quests, cutscenes, audio, etc.)
            base.OnInteractPress(interactor);

            // Tạo một "hộp" mới
            SceneChangeData data = new SceneChangeData
            {
                SceneName = _nextSceneName,
                TargetSpawnID = _targetSpawnID
            };

            // Gửi cả cái "hộp" đi
            EventManager.Notify(GameEvents.SceneTransition.OnSceneChangeRequested, data);
        }

    }
}