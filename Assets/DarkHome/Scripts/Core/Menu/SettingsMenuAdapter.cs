using UnityEngine;
using UnityEngine.UI;

namespace DarkHome
{
    public class SettingsMenuAdapter : MonoBehaviour
    {
        public static SettingsMenuAdapter Instance { get; private set; }

        [Header("--- TAB PANELS (Nội dung) ---")]
        [SerializeField] private GameObject _panelVideo;
        [SerializeField] private GameObject _panelGame;
        [SerializeField] private GameObject _panelControls; // Nếu có dùng KeyBindings

        [Header("--- HIGHLIGHT LINES (Gạch chân trang trí) ---")]
        [SerializeField] private GameObject _lineVideo;
        [SerializeField] private GameObject _lineGame;
        [SerializeField] private GameObject _lineControls;

        [Header("--- TAB BUTTONS (Nút bấm) ---")]
        [SerializeField] private Button _btnTabVideo;
        [SerializeField] private Button _btnTabGame;
        [SerializeField] private Button _btnTabControls;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_btnTabVideo) _btnTabVideo.onClick.AddListener(OpenVideoTab);
            if (_btnTabGame) _btnTabGame.onClick.AddListener(OpenGameTab);
            if (_btnTabControls) _btnTabControls.onClick.AddListener(OpenControlsTab);
        }

        private void OnEnable()
        {
            OpenVideoTab();
        }

        #region LOGIC CHUYỂN TAB (Bê từ UIMenuManager sang)

        private void DisableAllTabs()
        {
            // Tắt hết Panel
            if (_panelVideo) _panelVideo.SetActive(false);
            if (_panelGame) _panelGame.SetActive(false);
            if (_panelControls) _panelControls.SetActive(false);

            // Tắt hết gạch chân
            if (_lineVideo) _lineVideo.SetActive(false);
            if (_lineGame) _lineGame.SetActive(false);
            if (_lineControls) _lineControls.SetActive(false);
        }

        public void OpenVideoTab()
        {
            DisableAllTabs();
            if (_panelVideo) _panelVideo.SetActive(true);
            if (_lineVideo) _lineVideo.SetActive(true);
        }

        public void OpenGameTab()
        {
            DisableAllTabs();
            if (_panelGame) _panelGame.SetActive(true);
            if (_lineGame) _lineGame.SetActive(true);
        }

        public void OpenControlsTab()
        {
            DisableAllTabs();
            if (_panelControls) _panelControls.SetActive(true);
            if (_lineControls) _lineControls.SetActive(true);
        }

        #endregion
    }
}