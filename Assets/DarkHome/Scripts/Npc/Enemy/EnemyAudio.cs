using UnityEngine;

namespace DarkHome
{
    [RequireComponent(typeof(AudioSource))] // Loa 3D
    public class EnemyAudio : MonoBehaviour
    {
        [Header("Local Sounds (3D)")]
        [SerializeField] private AudioClip _screamClip;
        [SerializeField] private AudioClip[] _footsteps;

        private AudioSource _localSource;

        private void Awake()
        {
            _localSource = GetComponent<AudioSource>();
        }

        // Xử lý âm thanh TẠI CHỖ (Local)
        public void PlayScream()
        {
            if (_screamClip) _localSource.PlayOneShot(_screamClip);
        }

        public void PlayFootstep()
        {
            if (_footsteps.Length > 0)
            {
                var clip = _footsteps[Random.Range(0, _footsteps.Length)];
                _localSource.PlayOneShot(clip, 0.5f);
            }
        }

        //  Gửi yêu cầu lên HỆ THỐNG (Global)
        // Chỉ đơn giản là nhấc máy gọi: "Alo, bật nhạc hộ em!"
        public void RequestChaseMusic()
        {
            if (ChaseSystem.Instance != null) ChaseSystem.Instance.AddChaser();
        }

        public void StopChaseMusicRequest()
        {
            if (ChaseSystem.Instance != null) ChaseSystem.Instance.RemoveChaser();
        }
    }
}