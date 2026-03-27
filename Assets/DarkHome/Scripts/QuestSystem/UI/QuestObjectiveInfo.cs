using UnityEngine;
using TMPro;

namespace DarkHome
{
    public class QuestObjectiveInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _descriptionText;
        private QuestObjective _questObj;

        public void Initialize(QuestObjective newQuestObj)
        {
            _questObj = newQuestObj;

            // Use localization for objective description
            string description = LocalizationManager.Instance.GetText(_questObj.DescriptionKey);
            _descriptionText.SetText(description);

            // Cập nhật trạng thái ngay lập tức
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_questObj == null || _descriptionText == null) return;

            // Kiểm tra trạng thái hoàn thành từ Property thông minh IsCompleted
            if (_questObj.IsCompleted)
            {
                // Bật gạch ngang (Bitwise OR) & Đổi màu xám
                _descriptionText.fontStyle |= FontStyles.Strikethrough;
                _descriptionText.color = Color.gray;
            }
            else
            {
                // Tắt gạch ngang (Bitwise AND NOT) & Màu trắng
                _descriptionText.fontStyle &= ~FontStyles.Strikethrough;
                _descriptionText.color = Color.white;
            }
        }

        // THÊM ĐOẠN NÀY ĐỂ UI TỰ CẬP NHẬT LIVE

        private void OnEnable()
        {
            // Đăng ký lắng nghe sự kiện khi mục tiêu thay đổi
            EventManager.AddObserver<QuestObjective>(GameEvents.Objective.OnObjectiveStatusChanged, OnObjectiveStatusChanged);
        }

        private void OnDisable()
        {
            // Hủy đăng ký khi tắt UI
            EventManager.RemoveListener<QuestObjective>(GameEvents.Objective.OnObjectiveStatusChanged, OnObjectiveStatusChanged);
        }

        private void OnObjectiveStatusChanged(QuestObjective changedObj)
        {
            // Nếu mục tiêu vừa thay đổi CHÍNH LÀ mục tiêu mà cái UI này đang hiển thị
            // Thì gọi hàm cập nhật lại giao diện (để gạch ngang)
            if (changedObj == _questObj)
            {
                UpdateDisplay();
            }
        }
    }
}