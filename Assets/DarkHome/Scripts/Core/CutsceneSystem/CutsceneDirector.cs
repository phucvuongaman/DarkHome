using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.Cinemachine;

namespace DarkHome
{
    public class CutsceneDirector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private bool _playOnStart = false;

        [Header("Next Action")]
        [Tooltip("ID của EventTrigger sẽ được kích hoạt sau khi Cutscene kết thúc. (VD: 'EVENT_WAKEUP_DONE')")]
        [SerializeField] private FlagData _finishEvent; // Đổi tên biến cho đúng bản chất

        private void Start()
        {
            if (_director == null) _director = GetComponent<PlayableDirector>();
            BindTimelineToMainCamera();

            if (_playOnStart) PlayCutscene();
        }

        private void BindTimelineToMainCamera()
        {
            if (_director == null || _director.playableAsset == null) return;
            var timelineAsset = (TimelineAsset)_director.playableAsset;

            var mainBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (mainBrain != null)
            {
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    if (track is CinemachineTrack)
                    {
                        _director.SetGenericBinding(track, mainBrain);
                        break;
                    }
                }
            }
        }

        public void PlayCutscene()
        {
            if (_director == null) return;
            _director.stopped += OnCutsceneStopped;

            if (InputManager.Instance != null) InputManager.Instance.TogglePlayerInput(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(null, 1.0f);

            _director.Play();
        }

        private void OnCutsceneStopped(PlayableDirector obj)
        {
            _director.stopped -= OnCutsceneStopped;

            Debug.Log("OnCutsceneStopped");

            if (InputManager.Instance != null) InputManager.Instance.TogglePlayerInput(true);


            if (!string.IsNullOrEmpty(_finishEvent.FlagID))
            {
                Debug.Log($"🎬 Cutscene xong -> Kích hoạt EventTrigger: {_finishEvent.FlagID}");

                // Gọi thẳng vào Manager. 
                // Vì hàm ActiveEvent nhận vào FlagData, ta tạo một cái FlagData giả tạm thời với ID trùng khớp
                if (EventTriggerManager.Instance != null)
                {
                    // Lưu ý: Cậu cần đảm bảo hàm ActiveEvent của cậu xử lý được logic này
                    // Dựa trên code cũ của cậu: ActiveEvent tìm trigger theo ID của Flag.
                    EventTriggerManager.Instance.ActiveEvent(_finishEvent);
                }
            }
            // ---------------------------------------------------

            Debug.Log("🏁 Cutscene Finished.");
        }
    }
}