using UnityEngine;

namespace DarkHome
{
    public class AudioZone : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("File nhạc sẽ phát khi bước vào vùng này")]
        [SerializeField] private AudioClip _bgmClip;

        [Tooltip("Có phải là nhạc nền (Music) không? Nếu tắt thì nó tính là Ambience")]
        [SerializeField] private bool _isMusic = true;

        [Tooltip("Tag của đối tượng kích hoạt (thường là Player)")]
        [SerializeField] private string _targetTag = "Player";

        [Header("Fade Settings")]
        [Tooltip("Thời gian chuyển nhạc (giây). Càng lớn chuyển càng êm.")]
        [SerializeField] private float _fadeTime = 2.0f;

        // Biến kiểm tra xem Player có đang ở trong vùng này không
        private bool _isActive = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_targetTag))
            {
                _isActive = true;
                if (_isMusic)
                {
                    // Phát nhạc vùng (Zone Music)
                    AudioManager.Instance.PlayMusic(_bgmClip, _fadeTime);
                }
                else
                {
                    AudioManager.Instance.PlayAmbience(_bgmClip, _fadeTime);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_targetTag))
            {
                _isActive = false;

                // RA KHỎI VÙNG -> QUAY VỀ NHẠC GỐC CỦA NGÀY HÔM ĐÓ
                if (_isMusic && AudioManager.Instance != null)
                {
                    AudioManager.Instance.ReturnToSceneMusic(_fadeTime);
                }
            }
        }
    }
}