using UnityEngine;
using System.Collections;

namespace DarkHome
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [Tooltip("Loa chuyên dùng để phát nhạc nền (Loop)")]
        [SerializeField] private AudioSource _musicSource;

        [Tooltip("Loa chuyên dùng để phát âm thanh môi trường như gió, tiếng ồn (Loop)")]
        [SerializeField] private AudioSource _ambienceSource;

        [Tooltip("Loa chuyên dùng để phát tiếng động ngắn như tiếng bước chân, tiếng va đập")]
        [SerializeField] private AudioSource _sfxSource;

        [Header("Settings")]
        [Tooltip("Thời gian chuyển nhạc (Fade) mặc định tính bằng giây")]
        [SerializeField] private float _defaultFadeTime = 1.5f;

        //Lưu trữ bài nhạc gốc của Scene (theo ngày)
        private AudioClip _baseSceneMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject);

            if (_musicSource) _musicSource.loop = true;
            if (_ambienceSource) _ambienceSource.loop = true;
        }

        // --- HÀM 1: SETUP NHẠC NỀN CHO SCENE (GỌI KHI MỚI VÀO SCENE) ---
        public void SetSceneMusic(AudioClip clip, float fadeTime = 2f)
        {
            // Lưu lại bài này làm "Bài gốc". Đi đâu thì đi, tí phải về bài này.
            _baseSceneMusic = clip;

            // Phát luôn
            PlayMusic(clip, fadeTime);
        }

        // --- HÀM 2: QUAY VỀ NHẠC NỀN GỐC (GỌI KHI RA KHỎI ZONE) ---
        public void ReturnToSceneMusic(float fadeTime = 2f)
        {
            if (_baseSceneMusic != null)
            {
                PlayMusic(_baseSceneMusic, fadeTime);
            }
        }

        // --- CÁC HÀM CŨ (ĐÃ CÓ TOOLTIP) ---

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySFXPitched(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return;
            GameObject tempAudio = new GameObject("TempSFX_Pitched");
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
            Destroy(tempAudio, clip.length + 0.1f);
        }

        public void PlayMusic(AudioClip newClip, float fadeDuration = -1f)
        {
            if (fadeDuration < 0) fadeDuration = _defaultFadeTime;
            // Nếu đang hát đúng bài này rồi thì thôi (tránh bị reset đoạn nhạc)
            if (_musicSource.clip == newClip) return;

            StartCoroutine(FadeTrack(_musicSource, newClip, fadeDuration));
        }

        public void PlayAmbience(AudioClip newClip, float fadeDuration = -1f)
        {
            if (fadeDuration < 0) fadeDuration = _defaultFadeTime;
            if (_ambienceSource.clip == newClip) return;

            StartCoroutine(FadeTrack(_ambienceSource, newClip, fadeDuration));
        }

        public bool IsPlaying(AudioClip clip) => _musicSource.clip == clip;

        private IEnumerator FadeTrack(AudioSource source, AudioClip newClip, float duration)
        {
            float startVolume = source.volume;
            if (startVolume <= 0.01f) startVolume = 1f;

            float time = 0;
            // Fade Out
            if (source.isPlaying)
            {
                while (time < duration / 2)
                {
                    time += Time.deltaTime;
                    source.volume = Mathf.Lerp(startVolume, 0, time / (duration / 2));
                    yield return null;
                }
            }

            source.volume = 0;
            source.Stop();
            source.clip = newClip;

            // Fade In
            if (newClip != null)
            {
                source.Play();
                time = 0;
                while (time < duration / 2)
                {
                    time += Time.deltaTime;
                    source.volume = Mathf.Lerp(0, startVolume, time / (duration / 2));
                    yield return null;
                }
                source.volume = startVolume;
            }
        }
    }
}