using System;
using UnityEngine;

namespace DarkHome
{
    public class ScannerTarget : MonoBehaviour
    {
        [Header("Target Transform")]
        [SerializeField] private Transform _target;


        [Header("View Settings")]
        [SerializeField] private float viewDistance = 5f;
        [SerializeField] private float viewAngle = 90f;

        [Tooltip("Layers chứa vật cản (Wall, Door, Obstacle) để check line of sight. KHÔNG bao gồm Player/NPC layers.")]
        public LayerMask obstacleMask;

        public Transform Target { get => _target; set => _target = value; }

        public bool CheckingTarget()
        {
            if (_target == null) return false;

            // Check khoảng cách (Distance)
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);
            if (distanceToTarget > viewDistance) return false;

            // Tính toán vị trí MẮT (Eye Position)
            float eyeHeight = 1.5f;
            Vector3 origin = transform.position + Vector3.up * eyeHeight;      // Mắt Enemy
            Vector3 targetPos = _target.position + Vector3.up * eyeHeight;     // Ngực Player

            // Tính hướng bắn (Direction)
            Vector3 directionVector = targetPos - origin;
            Vector3 directionNormalized = directionVector.normalized; // Chuẩn hóa để lấy hướng chuẩn

            // Check góc nhìn (Angle)
            // Dùng directionNormalized để tính góc chính xác hơn
            float angleToTarget = Vector3.Angle(transform.forward, directionNormalized);
            if (angleToTarget > viewAngle / 2f) return false;

            // Check vật cản (Raycast) - QUAN TRỌNG
            // obstacleMask: Layer chứa Tường, Cửa, Vật cản (KHÔNG chứa Player)
            // Nếu bắn trúng vật cản -> Trả về false (Không nhìn thấy)

            float distToTarget = Vector3.Distance(origin, targetPos);

            if (Physics.Raycast(origin, directionNormalized, out RaycastHit hit, distToTarget, obstacleMask))
            {
                // Bắn trúng cái gì đó (Tường, Cột...) trước khi tới Player
                Debug.DrawLine(origin, hit.point, Color.red); // Vẽ tia đỏ báo lỗi
                // Debug.Log("Bị chặn bởi: " + hit.collider.name);
                return false;
            }

            // Nếu Raycast KHÔNG trúng vật cản nào -> Đường thoáng -> Nhìn thấy
            Debug.DrawLine(origin, targetPos, Color.green); // Vẽ tia xanh xác nhận
            return true;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f); // Xanh lá nhạt
            Gizmos.DrawWireSphere(transform.position, viewDistance);

            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewDistance);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewDistance);
        }
        private void OnDrawGizmos()
        {
            if (_target == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _target.position);
        }
#endif

    }
}