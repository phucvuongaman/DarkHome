/////////////////////////////
// Hardcode để test ending //
/////////////////////////////
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

namespace DarkHome
{

    public class EndingScreenManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMPro.TextMeshProUGUI _endingTitle;
        [SerializeField] private TMPro.TextMeshProUGUI _endingSubtitle;
        [SerializeField] private TMPro.TextMeshProUGUI _storyText;

        [Header("Settings")]
        [SerializeField] private float _fadeSpeed = 0.8f;
        [SerializeField] private float _displayTime = 8f;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            // Ẩn bằng CanvasGroup, KHÔNG dùng SetActive(false)
            // SetActive(false) → OnDisable → unsubscribe → StoryEvent không được nghe!
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            // Lắng nghe trực tiếp từ FlagManager — bypass EventTriggerManager chain
            EventManager.AddObserver<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<FlagData>(GameEvents.Flag.OnFlagChanged, OnFlagChanged);
        }

        private void OnFlagChanged(FlagData flag)
        {
            switch (flag.FlagID)
            {
                case "C1_ENDING_FALSE_COMFORT":
                    BuildAndShow("cutscene_ending_false_comfort");
                    break;
                case "C1_ENDING_CONTINUE":
                    BuildAndShow("cutscene_ending_continue");
                    break;
                case "C1_ENDING_TRUTH":
                    BuildAndShow("cutscene_ending_truth");
                    break;
            }
        }

        private void BuildAndShow(string endingId)
        {
            // --- ENDING TITLE ---
            string title = "";
            string subtitle = "";

            switch (endingId)
            {
                case "cutscene_ending_false_comfort":
                    title = "ENDING I";
                    subtitle = "False Comfort\n\nBạn chọn ở lại.\nTrong bóng tối quen thuộc,\nbạn tìm thấy sự bình yên giả.";
                    break;
                case "cutscene_ending_continue":
                    title = "ENDING II";
                    subtitle = "The Loop Continues\n\nBạn không đầu hàng, cũng không thoát được.\nVòng lặp vẫn còn đó — chờ bạn.";
                    break;
                case "cutscene_ending_truth":
                    title = "ENDING III";
                    subtitle = "Truth\n\nBạn nhìn thẳng vào sự thật.\nDù đau đớn — bạn thấy rõ.";
                    break;
            }

            if (_endingTitle != null) _endingTitle.text = title;
            if (_endingSubtitle != null) _endingSubtitle.text = subtitle;

            // --- YOUR STORY (từ flags đã thu thập) ---
            if (_storyText != null)
                _storyText.text = BuildStoryLog();

            StartCoroutine(FadeIn());
        }

        private string BuildStoryLog()
        {
            var sb = new StringBuilder();
            sb.AppendLine("— Hành trình của bạn —\n");

            // Day 1
            Append(sb, "C1_PROGRESS_DAY1_CLEANED", "✓ Bạn đã dọn nhà khi được nhờ.");
            Append(sb, "C1_STORY_EXAMINED_PHOTO1", "✓ Bạn chú ý bức ảnh gia đình.");

            // Day 2
            Append(sb, "C1_STORY_NOTICED_BLUR", "✓ Bạn nhận ra ảnh đang mờ đi.");
            Append(sb, "C1_PROGRESS_DAY2_PHOTO_ASKED", "✓ Bạn hỏi Mika về tấm ảnh.");

            // Day 3
            Append(sb, "C1_STORY_NOTICED_LOOP", "✓ Bạn phát hiện vòng lặp thời gian.");
            Append(sb, "C1_STORY_SAW_NEWS", "✓ Bạn đã xem bản tin.");
            Append(sb, "C1_PROGRESS_DAY3_CONFRONTED_NEWS", "✓ Bạn đối chất thẳng với Mika.");

            // Day 4
            Append(sb, "C1_PROGRESS_DAY4_READ_DIARY", "✓ Bạn đọc nhật ký bí mật.");
            Append(sb, "C1_PROGRESS_DAY4_MIRROR_TRUTH", "✓ Bạn đối mặt với gương sự thật.");
            Append(sb, "C1_PROGRESS_DAY4_TRIED_ESCAPE", "✓ Bạn đã cố thoát ra ngoài.");
            Append(sb, "C1_PROGRESS_DAY4_ACCEPTANCE", "✓ Bạn tìm thấy sự chấp nhận.");

            // Day 5
            Append(sb, "C1_PROGRESS_DAY5_FINAL_NOTE", "✓ Bạn tìm thấy tờ ghi chú cuối cùng.");

            // Lore
            Append(sb, "C1_LORE_DIARY", "✓ [Lore] Bạn đọc Nhật Ký tối.", true);
            Append(sb, "C1_LORE_NOTE01", "✓ [Lore] Ghi chú bí ẩn #1.", true);
            Append(sb, "C1_LORE_NOTE03", "✓ [Lore] Ghi chú bí ẩn #3.", true);

            return sb.ToString();
        }

        /// <param name="quiet">Nếu true, chỉ hiển thị khi flag có mặt. Không hiển thị "✗".</param>
        private void Append(StringBuilder sb, string flagId, string text, bool quiet = false)
        {
            var fm = FlagManager.Instance;
            if (fm == null) return;

            bool has = fm.HasFlag(new FlagData(flagId, EFlagScope.Local));
            if (has)
                sb.AppendLine(text);
            else if (!quiet)
                sb.AppendLine(text.Replace("✓", "✗").Replace("Bạn đã", "Bạn không").Replace("Bạn ", "Bạn không "));
        }

        private IEnumerator FadeIn()
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * _fadeSpeed;
                _canvasGroup.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }
    }
}
