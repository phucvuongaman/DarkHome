using UnityEngine;
using UnityEngine.InputSystem;

namespace DarkHome
{
    public class MouseMoveObject : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _defaultDistance = 5f;

        // Không cache cố định trong Awake nữa
        private Camera _currentCamera;

        private void Update()
        {
            PinObjectToMouse();
        }

        void PinObjectToMouse()
        {
            if (Mouse.current == null) return;

            // Luôn cập nhật Camera mới nhất
            // Nếu camera hiện tại bị tắt hoặc null, tìm cái mới ngay
            if (_currentCamera == null || !_currentCamera.gameObject.activeInHierarchy)
            {
                _currentCamera = Camera.main;

                // Fallback: Nếu Camera Menu không gắn tag MainCamera, tìm đại cái nào đang chạy
                if (_currentCamera == null)
                {
                    _currentCamera = FindFirstObjectByType<Camera>();
                }
            }

            if (_currentCamera == null) return; // Vẫn không tìm thấy thì bó tay

            // Lấy vị trí chuột
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            // Tính toán vị trí dựa trên CAMERA HIỆN TẠI
            Vector3 screenPosition = new Vector3(mouseScreenPos.x, mouseScreenPos.y, _defaultDistance);
            Vector3 worldPos = _currentCamera.ScreenToWorldPoint(screenPosition);

            // Gán vị trí
            transform.position = worldPos;

            // (Optional) Quay mặt về phía camera để dễ nhìn
            transform.LookAt(transform.position + _currentCamera.transform.rotation * Vector3.forward,
                             _currentCamera.transform.rotation * Vector3.up);
        }
    }
}