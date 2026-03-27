using UnityEngine;

namespace DarkHome
{
    public class ChaseSystem : MonoBehaviour
    {
        public static ChaseSystem Instance { get; private set; }

        [Header("Global Config")]
        [SerializeField] private AudioClip _chaseMusic; // Nhạc đuổi bắt chung

        // Biến này lưu "Nhạc nền hiện tại" của khu vực. 
        // Khi hết đuổi, ta sẽ quay về bài này.
        private AudioClip _currentZoneMusic;
        private int _enemyCount = 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }


        // Hàm này để các Khu vực/Scene đăng ký nhạc của họ
        public void SetZoneMusic(AudioClip clip)
        {
            _currentZoneMusic = clip;

            // Nếu đang không bị đuổi, thì chuyển nhạc ngay
            if (_enemyCount == 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(_currentZoneMusic);
            }
        }

        public void AddChaser()
        {
            _enemyCount++;
            if (_enemyCount == 1)
            {
                // Bị đuổi -> Bật nhạc hành động
                AudioManager.Instance.PlayMusic(_chaseMusic);
            }
        }

        public void RemoveChaser()
        {
            _enemyCount--;
            if (_enemyCount <= 0)
            {
                _enemyCount = 0;
                // Hết bị đuổi -> Quay về nhạc khu vực đã lưu
                AudioManager.Instance.PlayMusic(_currentZoneMusic);
            }
        }
    }
}