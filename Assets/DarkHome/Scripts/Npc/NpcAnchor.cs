using UnityEngine;

namespace DarkHome
{
    public enum ENpcPose
    {
        Stand,
        Sit,
        Guard,
        Read
    }

    public class NpcAnchor : MonoBehaviour
    {
        [Header("Cấu hình tại điểm này")]
        public ENpcPose Pose = ENpcPose.Stand; // Đến đây thì đứng hay ngồi?

        [Tooltip("Có bắt buộc quay mặt theo hướng mũi tên xanh không")]
        public bool SnapRotation = false;

        // Vẽ Gizmo để dễ nhìn trong Scene
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
        }
    }
}