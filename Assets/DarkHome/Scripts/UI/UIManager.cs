using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DarkHome
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Public property để Settings có thể toggle HUD
        public GameObject HudCanvas => _hudCanvas;

        [Header("Managed Canvases")]
        [Tooltip("Canvas chứa các yếu tố HUD (chấm, text tương tác, Quest HUD)")]
        [SerializeField] private GameObject _hudCanvas;




        [Tooltip("Canvas chứa Hội thoại (Dialogue)")]
        [SerializeField] private GameObject _dialogueCanvas;

        [Header("Managed Panels")]
        [Tooltip("Kéo Panel CHÍNH của Sổ tay Nhiệm vụ vào đây")]
        [SerializeField] private GameObject _questLogPanel;
        [Tooltip("Panel Cài đặt (Settings)")]
        [SerializeField] private GameObject _settingsPanel;


        [Header("Managed UI")]
        [SerializeField] private GameObject interactTextObject;
        [SerializeField] private ChoiceUI _choiceUI;
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private QuestUI _questUI;



        private TextMeshProUGUI _interactTextTMP; // Biến để lưu tham chiếu




        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject);


            if (interactTextObject != null)
            {
                _interactTextTMP = interactTextObject.GetComponent<TextMeshProUGUI>();
                if (_interactTextTMP == null)
                {
                    Debug.LogError("Interact Text GameObject does not have a TextMeshProUGUI component!");
                }
                interactTextObject.SetActive(false); // Ẩn đi khi bắt đầu
            }
            else
            {
                Debug.LogError("Interact Text GameObject is not assigned in the UIManager Inspector.");
            }
        }


        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;

            InputManager.onEscapePressed += TogglePausePanel;
            InputManager.onJournalPressed += ToggleJournalPanel;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;

            InputManager.onEscapePressed -= TogglePausePanel;
            InputManager.onJournalPressed -= ToggleJournalPanel;
        }

        #region Toggle Panels
        public void TogglePausePanel()
        {
            if (GameManager.Instance.CurrentState == GameState.MainMenu)
            {
                _questLogPanel.SetActive(false);
                return;
            }
            // Nếu Sổ tay đang mở, HÃY TẮT NÓ ĐI
            if (_questLogPanel != null && _questLogPanel.activeSelf)
            {
                _questLogPanel.SetActive(false);
            }

            // Logic toggle Setting như cũ
            bool isOpening = !_settingsPanel.activeSelf;
            _settingsPanel.SetActive(isOpening);

            // Ra lệnh cho GameManager
            GameManager.Instance.UpdateGameState(
                isOpening ? GameState.Paused : GameState.Gameplay);
        }

        public void ToggleJournalPanel()
        {
            if (GameManager.Instance.CurrentState == GameState.MainMenu)
            {
                _settingsPanel.SetActive(false);
                return;
            }
            // Nếu Setting đang mở, HÃY TẮT NÓ ĐI
            if (_settingsPanel != null && _settingsPanel.activeSelf)
            {
                _settingsPanel.SetActive(false);
            }

            // Logic toggle Journal như cũ
            bool isOpening = !_questLogPanel.activeSelf;
            _questLogPanel.SetActive(isOpening);

            // Chúng ta giữ nó lại và gọi hàm của QuestUI
            if (isOpening && _questUI != null)
            {
                _questUI.UpdateQuestList();
            }

            // Ra lệnh cho GameManager
            GameManager.Instance.UpdateGameState(
                isOpening ? GameState.Paused : GameState.Gameplay);
        }
        #endregion

        #region Auto cast function

        /// <summary>
        /// Hàm này sẽ được gọi khi GameState thay đổi
        /// Sử lý trạng thái của các canvas để không đè lên các ui khác khi ở menu
        /// </summary>
        /// <param name="newState">GameManager sẽ truyền state vào</param>
        private void HandleGameStateChanged(GameState newState)
        {
            // Ẩn tất cả các panel có thể xung đột trước
            _hudCanvas?.SetActive(false);
            // _questLogCanvas?.SetActive(false); // Sổ tay Quest
            // _questLogPanel?.SetActive(false);
            _dialogueCanvas?.SetActive(false); // Khung hội thoại
            // _settingsPanel?.SetActive(false); // Panel cài đặt

            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    // Ở Main Menu, mọi thứ đều ẩn và chuột hiện ra
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    // _questLogPanel?.SetActive(false);
                    break;

                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    // Khi chơi, bật HUD, khóa chuột và ẩn con trỏ
                    _hudCanvas?.SetActive(true);
                    _dialogueCanvas?.SetActive(true); // Bật sẵn canvas để nó lắng nghe event
                    _settingsPanel?.SetActive(false);
                    _questLogPanel?.SetActive(false);

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    // Khi Pause, hiện panel cài đặt và hiện chuột
                    // _settingsPanel?.SetActive(true);
                    // _hudCanvas?.SetActive(true); // (Có thể giữ HUD nếu muốn)

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }
        #endregion

        #region API function

        // DialogueFlowController callback
        public void ShowDialogue(DialogueNode node)
        {
            // _dialogueUI.DisplayDialogue(node);
            EventManager.Notify(GameEvents.DiaLog.ReadDialogue, node);
        }
        public void HideDialogue()
        {
            // _dialogueUI.HideDialogue();
            EventManager.Notify(GameEvents.DiaLog.EndDialogue);
        }
        public void ShowChoices(List<Choice> validChoices)
        {
            _choiceUI.DisplayChoices(validChoices);
        }

        public void HideChoices()
        {
            _choiceUI.EndChoice();
        }

        // Interactor callback
        public void ShowInteractText(string text)
        {
            if (_interactTextTMP != null)
            {
                _interactTextTMP.text = text;
                interactTextObject.SetActive(true);
            }
        }
        public void HideInteractText()
        {
            if (interactTextObject != null)
                interactTextObject.SetActive(false);
        }

        #endregion
    }
}