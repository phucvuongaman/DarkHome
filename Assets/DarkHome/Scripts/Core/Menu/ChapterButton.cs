using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DarkHome
{
    /// <summary>
    /// Individual chapter button in Chapter Selection panel
    /// Handles visual states (locked/unlocked) and selection feedback
    /// </summary>
    public class ChapterButton : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Button component của chapter button này")]
        [SerializeField] private Button _button;
        [Tooltip("Text hiển thị số chapter (1, 2, 3...)")]
        [SerializeField] private TMP_Text _chapterNumber;
        [Tooltip("Text hiển thị tên chapter (vd: 'Into the Dark')")]
        [SerializeField] private TMP_Text _chapterTitle;
        [Tooltip("Background image của button (để đổi màu locked/unlocked)")]
        [SerializeField] private Image _backgroundImage;
        [Tooltip("Icon 🔒 hiện khi chapter bị lock (disable GameObject này)")]
        [SerializeField] private GameObject _lockIcon;
        [Tooltip("Icon ✅ hiện khi chapter đã hoàn thành (disable GameObject này)")]
        [SerializeField] private GameObject _checkmarkIcon;
        [Tooltip("Image preview của chapter (bên trái button)")]
        [SerializeField] private Image _previewImage;

        [Header("Visual States")]
        [Tooltip("Màu button khi chapter unlocked (mặc định: trắng)")]
        [SerializeField] private Color _unlockedColor = Color.white;
        [Tooltip("Màu button khi chapter locked (mặc định: xám mờ)")]
        [SerializeField] private Color _lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [Tooltip("Border highlight khi button được chọn (disable GameObject này)")]
        [SerializeField] private GameObject _highlightBorder;

        private ChapterDataSO _chapterData;
        private bool _isUnlocked;
        private ChapterSelectionController _controller;

        /// <summary>
        /// Initialize button with chapter data
        /// </summary>
        public void Init(ChapterDataSO chapterData, bool isUnlocked, ChapterSelectionController controller)
        {
            _chapterData = chapterData;
            _isUnlocked = isUnlocked;
            _controller = controller;

            UpdateVisuals();

            // Register click listener
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners(); // Clear old listeners
                _button.onClick.AddListener(OnClick);
            }
        }

        /// <summary>
        /// Update visual state based on unlock status
        /// </summary>
        private void UpdateVisuals()
        {
            if (_chapterData == null) return;

            // --- Chapter Info ---
            if (_chapterNumber != null)
                _chapterNumber.text = _chapterData.chapterNumber.ToString();

            if (_chapterTitle != null)
                _chapterTitle.text = _chapterData.chapterName;

            // --- Lock/Unlock Visual ---
            if (_lockIcon != null)
                _lockIcon.SetActive(!_isUnlocked);

            if (_backgroundImage != null)
                _backgroundImage.color = _isUnlocked ? _unlockedColor : _lockedColor;

            // --- Preview Image ---
            if (_previewImage != null && _chapterData.previewSprite != null)
            {
                _previewImage.sprite = _chapterData.previewSprite;
                _previewImage.color = _isUnlocked ? Color.white : _lockedColor;
            }

            // --- Button Interactable ---
            if (_button != null)
                _button.interactable = _isUnlocked;
        }

        /// <summary>
        /// Handle button click
        /// </summary>
        private void OnClick()
        {
            if (_controller != null)
            {
                _controller.OnChapterSelected(_chapterData, _isUnlocked);

                // Visual feedback (highlight this button)
                SetSelected(true);
            }
        }

        /// <summary>
        /// Set selected visual state
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (_highlightBorder != null)
                _highlightBorder.SetActive(selected);
        }

        /// <summary>
        /// Update unlock status (for when player unlocks new chapter)
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            _isUnlocked = unlocked;
            UpdateVisuals();
        }

        /// <summary>
        /// Play shake animation (when clicking locked chapter)
        /// </summary>
        public void PlayLockedFeedback()
        {
            // Simple shake using animation
            if (_button != null && _button.TryGetComponent<Animator>(out var animator))
            {
                animator.SetTrigger("Shake");
            }
            else
            {
                // Fallback: Simple scale pulse
                StartCoroutine(ShakeFeedback());
            }
        }

        private System.Collections.IEnumerator ShakeFeedback()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pulse effect
                float scale = 1f + Mathf.Sin(t * Mathf.PI * 4) * 0.05f;
                transform.localScale = originalScale * scale;

                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}
