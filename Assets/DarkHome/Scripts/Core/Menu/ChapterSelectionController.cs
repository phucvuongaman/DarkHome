using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DarkHome
{
    /// <summary>
    /// Controller for Chapter Selection Panel
    /// Manages chapter buttons, unlock states, preview display
    /// </summary>
    public class ChapterSelectionController : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("RectTransform của Pan_ChapterSelection (panel chính) - dùng cho slide animation")]
        [SerializeField] private RectTransform _panelTransform;
        [Tooltip("Container chứa các chapter buttons (ChapterButtonContainer GameObject)")]
        [SerializeField] private Transform _chapterButtonContainer;
        [Tooltip("Prefab ChapterButton để spawn từng button chapter")]
        [SerializeField] private GameObject _chapterButtonPrefab;

        [Header("Preview Panel")]
        [Tooltip("Preview panel GameObject (PreviewPanel) - disable initially")]
        [SerializeField] private GameObject _previewPanel;
        [Tooltip("Text title của preview (vd: 'CHAPTER 1: INTO THE DARK')")]
        [SerializeField] private TMP_Text _previewTitle;
        [Tooltip("Text mô tả chapter trong preview panel")]
        [SerializeField] private TMP_Text _previewDescription;
        [Tooltip("Image preview trong preview panel (optional - có thể để trống)")]
        [SerializeField] private Image _previewImage;
        [Tooltip("Button 'PLAY CHAPTER' để bắt đầu chapter đã chọn")]
        [SerializeField] private Button _playChapterButton;

        [Header("Panel Animation")]
        [Tooltip("Thời gian slide panel vào từ phải (giây) - mặc định 0.4s")]
        [SerializeField] private float _slideInDuration = 0.4f;
        [Tooltip("Thời gian slide panel ra ngoài phải (giây) - mặc định 0.3s")]
        [SerializeField] private float _slideOutDuration = 0.3f;

        [Header("Audio (Optional)")]
        [Tooltip("Audio khi select chapter (optional - có thể để trống)")]
        [SerializeField] private AudioSource _selectSound;
        [Tooltip("Audio khi click vào chapter bị lock (optional - có thể để trống)")]
        [SerializeField] private AudioSource _lockedSound;

        private ChapterButton[] _chapterButtons;
        private ChapterDataSO _selectedChapter;
        private bool _hasSlid = false; // Track if already slid in

        private void Start()
        {
            PopulateChapterButtons();

            if (_playChapterButton != null)
                _playChapterButton.onClick.AddListener(OnPlaySelectedChapter);

            // Hide preview initially
            if (_previewPanel != null)
                _previewPanel.SetActive(false);
        }

        private void OnEnable()
        {
            // Only slide first time, avoid re-sliding
            if (!_hasSlid && _panelTransform != null)
            {
                StartCoroutine(SlideInPanel());
            }
        }

        /// <summary>
        /// Create chapter buttons from ChapterManager's progression list
        /// </summary>
        private void PopulateChapterButtons()
        {
            if (ChapterManager.Instance == null)
            {
                Debug.LogError("❌ ChapterManager not found!");
                return;
            }

            // Get all chapters
            var allChapters = ChapterManager.Instance.GetAllChapters();
            if (allChapters == null || allChapters.Count == 0)
            {
                Debug.LogWarning("⚠️ No chapters found in ChapterManager!");
                return;
            }

            // Get unlock progress
            int highestUnlocked = ChapterManager.Instance.GetHighestUnlockedChapter();

            // Count existing buttons
            int existingButtons = _chapterButtonContainer.childCount;

            // Create or update buttons
            _chapterButtons = new ChapterButton[allChapters.Count];

            for (int i = 0; i < allChapters.Count; i++)
            {
                ChapterDataSO chapterData = allChapters[i];
                bool isUnlocked = (i <= highestUnlocked);

                GameObject buttonObj;

                // Reuse existing button or create new
                if (i < existingButtons)
                {
                    buttonObj = _chapterButtonContainer.GetChild(i).gameObject;
                }
                else
                {
                    buttonObj = Instantiate(_chapterButtonPrefab, _chapterButtonContainer);
                    buttonObj.name = $"ChapterButton_{i + 1}";
                }

                // Init button
                ChapterButton button = buttonObj.GetComponent<ChapterButton>();
                if (button != null)
                {
                    button.Init(chapterData, isUnlocked, this);
                    _chapterButtons[i] = button;
                }
                else
                {
                    Debug.LogError($"❌ ChapterButton component missing on {buttonObj.name}!");
                }
            }

            Debug.Log($"✅ Created {allChapters.Count} chapter buttons, {highestUnlocked + 1} unlocked");
        }

        /// <summary>
        /// Called by ChapterButton when clicked
        /// </summary>
        public void OnChapterSelected(ChapterDataSO chapterData, bool isUnlocked)
        {
            if (!isUnlocked)
            {
                // Locked feedback
                Debug.Log($"🔒 Chapter {chapterData.chapterNumber} is locked!");

                if (_lockedSound != null)
                    _lockedSound.Play();

                // TODO: Show tooltip "Complete previous chapter to unlock"
                return;
            }

            // Update selection
            _selectedChapter = chapterData;

            // Play select sound
            if (_selectSound != null)
                _selectSound.Play();

            // Update preview
            UpdatePreviewPanel(chapterData);

            Debug.Log($"✅ Selected: {chapterData.chapterName}");
        }

        /// <summary>
        /// Update preview panel with chapter info
        /// </summary>
        private void UpdatePreviewPanel(ChapterDataSO chapterData)
        {
            if (_previewPanel == null) return;

            _previewPanel.SetActive(true);

            if (_previewTitle != null)
                _previewTitle.text = $"CHAPTER {chapterData.chapterNumber}: {chapterData.chapterName.ToUpper()}";

            if (_previewDescription != null)
                _previewDescription.text = chapterData.chapterDescription;

            if (_previewImage != null && chapterData.previewSprite != null)
            {
                _previewImage.sprite = chapterData.previewSprite;
                _previewImage.enabled = true;
            }
            else if (_previewImage != null)
            {
                _previewImage.enabled = false;
            }
        }

        /// <summary>
        /// Play selected chapter (called by PLAY CHAPTER button)
        /// </summary>
        private void OnPlaySelectedChapter()
        {
            if (_selectedChapter == null)
            {
                Debug.LogWarning("⚠️ No chapter selected!");
                return;
            }

            Debug.Log($"🚀 Loading {_selectedChapter.chapterName}...");

            // Load chapter via ChapterManager
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.LoadChapter(_selectedChapter);
            }

            // Transition to game scene
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionTo(
                    _selectedChapter.sceneName,
                    _selectedChapter.startSpawnID
                );
            }
        }

        /// <summary>
        /// Refresh unlock status (call after completing a chapter)
        /// </summary>
        public void RefreshUnlockStatus()
        {
            if (_chapterButtons == null || ChapterManager.Instance == null) return;

            int highestUnlocked = ChapterManager.Instance.GetHighestUnlockedChapter();

            for (int i = 0; i < _chapterButtons.Length; i++)
            {
                bool isUnlocked = (i <= highestUnlocked);
                _chapterButtons[i].SetUnlocked(isUnlocked);
            }

            Debug.Log($"🔄 Refreshed unlock status: {highestUnlocked + 1} chapters unlocked");
        }

        #region === Panel Animation ===

        /// <summary>
        /// Slide panel in from right (called by OnEnable)
        /// </summary>
        private System.Collections.IEnumerator SlideInPanel()
        {
            if (_panelTransform == null) yield break;

            // Start position: Off-screen right
            float screenWidth = Screen.width;
            Vector2 startPos = new Vector2(screenWidth, 0);

            // End position: (0,0) = anchor position!
            // Anchors Min(0.44,0) Max(1,1) đã define vị trí panel rồi
            Vector2 endPos = Vector2.zero;

            _panelTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < _slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _slideInDuration;

                // EaseOutCubic for smooth deceleration
                t = 1f - Mathf.Pow(1f - t, 3f);

                _panelTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            _panelTransform.anchoredPosition = endPos;
            _hasSlid = true; // Mark as slid
        }

        /// <summary>
        /// Slide panel out to right (called by BACK button)
        /// </summary>
        private System.Collections.IEnumerator SlideOutPanel()
        {
            if (_panelTransform == null) yield break;

            Vector2 startPos = _panelTransform.anchoredPosition;
            Vector2 endPos = new Vector2(Screen.width, 0);

            float elapsed = 0f;
            while (elapsed < _slideOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _slideOutDuration;

                // Linear easing for slide out (faster)
                _panelTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            // Disable panel after animation
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Public method to close panel (call from BACK button)
        /// </summary>
        public void ClosePanel()
        {
            StartCoroutine(SlideOutPanel());
        }

        #endregion
    }
}
