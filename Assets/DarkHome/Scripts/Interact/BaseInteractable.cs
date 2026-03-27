using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable, IFocusable
    {
        [SerializeField] private string _id = System.Guid.NewGuid().ToString();
        [ContextMenu("Generate New ID")] // Tạo menu chuột phải
        private void GenerateId()
        {
            _id = System.Guid.NewGuid().ToString();
            // Đánh dấu object đã thay đổi để Unity cho phép Save scene
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            Debug.Log($"Generated New ID for {name}: {_id}");
        }
        [SerializeField] private string _interactableName;

        public virtual string Id { get => _id; set => _id = value; }
        public virtual string InteractableName { get => _interactableName; set => _interactableName = value; }
        public abstract InteractableType InteractType { get; set; }
        public virtual void OnFocus() { }
        public virtual void OnLoseFocus() { }
        public virtual void OnInteractPress(Interactor interactor) { }
        public virtual void OnInteractHold(Interactor interactor) { }
        public virtual void OnInteractRelease(Interactor interactor) { }
    }

}