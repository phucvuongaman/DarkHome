using UnityEngine;

namespace DarkHome
{
    public class SceneMusicStarter : MonoBehaviour
    {
        [Tooltip("Nhạc nền của ngày hôm nay. Sẽ phát ngay khi GameObject này được Active.")]
        [SerializeField] private AudioClip _dayMusic;

        [Tooltip("Thời gian Fade vào lúc bắt đầu Scene")]
        [SerializeField] private float _fadeTime = 1f;

        private void Start()
        {
            if (AudioManager.Instance != null && _dayMusic != null)
            {
                // Gọi hàm SetSceneMusic mới -> Vừa phát nhạc, vừa lưu làm mốc để quay về
                AudioManager.Instance.SetSceneMusic(_dayMusic, _fadeTime);
                Debug.Log($"🎵 Đã set nhạc nền ngày hôm nay: {_dayMusic.name}");
            }
        }
    }
}