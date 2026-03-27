using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkHome
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask detectionLayer;


        private BaseInteractable _currentTarget;

        public BaseInteractable CurrentTarget => _currentTarget;


        private Camera CurrentCamera
        {
            get
            {
                // Ưu tiên 1: Lấy từ CameraController (Hệ thống chuẩn của cậu)
                if (CameraController.Instance != null && CameraController.Instance.UnityCamera != null)
                {
                    return CameraController.Instance.UnityCamera;
                }

                // Ưu tiên 2: Chữa cháy nếu chưa có Controller (Lấy Camera.main)
                return Camera.main;
            }
        }

        // Gọi trong các player state cần sử dụng
        public void OnFocus()
        {
            if (CurrentCamera == null) return;

            Ray ray = CurrentCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            // 1. Bắn tia Ray
            if (Physics.Raycast(ray, out hit, maxDistance, detectionLayer))
            {
                var newTarget = hit.transform.GetComponent<BaseInteractable>();

                // Kiểm tra kỹ điều kiện
                bool isValidTarget = newTarget != null
                                     && (interactableLayer.value & (1 << hit.transform.gameObject.layer)) != 0;

                if (isValidTarget)
                {
                    if (newTarget != _currentTarget) FocusTarget(newTarget);
                }
                else
                {
                    // 👇 CÁI ELSE BẠN NÓI ĐÂY: Nhìn trúng tường -> Xóa ngay
                    ClearTarget();
                }
            }
            else
            {
                // 2. Nhìn vào hư không -> Xóa ngay
                ClearTarget();
            }
        }

        // Đăng ký với nút E InputManager ở Context
        public void InteractPress()
        {
            if (_currentTarget != null)
            {
                // 👇 FIX THÊM: Kiểm tra xem vật đó có bị hủy (Destroy) bất ngờ không
                if (_currentTarget == null || _currentTarget.gameObject == null)
                {
                    ClearTarget();
                    return;
                }

                _currentTarget.OnInteractPress(this);
            }
            // else
            // {
            //     Debug.Log("No interactable target in range.");
            // }
        }
        // Đăng ký với nút E InputManager ở Context
        public void InteractHold()
        {
            if (_currentTarget != null)
            {
                if (_currentTarget == null || _currentTarget.gameObject == null)
                {
                    ClearTarget();
                    return;
                }
                _currentTarget.OnInteractHold(this);
            }
            // else
            // {
            //     Debug.Log("No interactable target to hold interaction with.");
            // }
        }

        private void FocusTarget(BaseInteractable newTarget)
        {
            if (_currentTarget != null) _currentTarget.OnLoseFocus();

            _currentTarget = newTarget;
            _currentTarget.OnFocus();
            UIManager.Instance.ShowInteractText(_currentTarget.InteractableName);
        }

        private void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnLoseFocus();
                _currentTarget = null;
                UIManager.Instance.HideInteractText();
            }
        }


        private void OnDisable()
        {
            ClearTarget();
        }
    }
}

