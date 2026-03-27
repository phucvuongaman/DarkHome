using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkHome
{
    public class SceneInfo : MonoBehaviour
    {
        public static SceneInfo Instance { get; private set; }

        [SerializeField] private string _sceneId;
        [SerializeField] private string _chapterId;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            // DontDestroyOnLoad(gameObject);

            _sceneId = SceneManager.GetActiveScene().name;
        }

        public string SceneId { get; }
        public string ChapterId { get; }

    }
}