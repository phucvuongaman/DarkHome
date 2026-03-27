using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkHome
{
    public class Bootstrapper : MonoBehaviour
    {
        // Gõ đúng tên scene Main Menu của bạn vào đây trong Inspector
        [SerializeField] private string _mainMenuSceneName = "Main Menu";

        private void Start()
        {
            // Ngay lập tức ra lệnh tải scene Main Menu
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}