// TODO: Cần test kỹ. một vài object dù đã xong rồi nhưng vẫn hiện lên.
// một vài object khi xong rồi thì không gạch ngang mà ẩn luôn.
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace DarkHome
{
    public class QuestObjectiveUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Nơi chứa các dòng chữ mục tiêu (Content)")]
        [SerializeField] private Transform _contentParent;

        [Tooltip("Prefab mẫu của dòng chữ mục tiêu")]
        [SerializeField] private TextMeshProUGUI _objectiveTextPrefab;

        public void DisplayObjectives(Quest quest)
        {
            // Xóa sạch các dòng chữ cũ
            foreach (Transform child in _contentParent)
            {
                Destroy(child.gameObject);
            }

            if (quest == null || quest.Objectives == null) return;

            // Duyệt qua từng mục tiêu để quyết định Hiển thị
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var obj = quest.Objectives[i];

                // Kiểm tra xem mục tiêu này đã xong chưa?
                bool isCompleted = FlagManager.Instance.HasAllFlags(obj.CompletionFlags);

                // Biến quyết định có vẽ dòng này ra màn hình không
                bool shouldShow = true;

                // --- LOGIC QUYẾT ĐỊNH ẨN/HIỆN (Dựa trên Mode) ---
                if (quest.DisplayMode == EQuestDisplayMode.Sequential)
                {
                    // === CHẾ ĐỘ TUẦN TỰ ===

                    // Multi-flag (VD: Explore 4 đồ) → gạch ngang khi xong để player thấy
                    // Single-flag (VD: Wake up, Talk to Mika) → ẩn luôn, next objective hiện
                    bool isMultiFlag = obj.CompletionFlags != null && obj.CompletionFlags.Count > 1;
                    if (isCompleted && !isMultiFlag) shouldShow = false;

                    // Nếu là mục tiêu sau (i > 0), phải kiểm tra mục tiêu trước
                    if (i > 0)
                    {
                        var prevObj = quest.Objectives[i - 1];
                        // Dùng IsCompleted (có check Count > 0) thay vì HasAllFlags trực tiếp
                        // → tránh cascade khi CompletionFlags rỗng (HasAllFlags([]) = true)
                        if (!prevObj.IsCompleted)
                        {
                            shouldShow = false;
                        }
                    }
                }
                else if (quest.DisplayMode == EQuestDisplayMode.Parallel)
                {
                    // === CHẾ ĐỘ SONG SONG ===
                    // Luôn hiện tất cả (Danh sách việc cần làm)
                    shouldShow = true;
                }
                else // Custom
                {
                    // === CHẾ ĐỘ TÙY CHỈNH (Cũ) ===
                    if (obj.RequiredFlagsToAppear != null && obj.RequiredFlagsToAppear.Count > 0)
                    {
                        if (!FlagManager.Instance.HasAllFlags(obj.RequiredFlagsToAppear))
                        {
                            shouldShow = false;
                        }
                    }

                    // Nếu xong rồi và có tích chọn ẩn -> thì ẩn
                    if (isCompleted && obj.HideOnCompletion) shouldShow = false;
                }

                // --- VẼ RA MÀN HÌNH ---
                if (shouldShow)
                {
                    TextMeshProUGUI textObj = Instantiate(_objectiveTextPrefab, _contentParent);

                    // Get localized objective description
                    string description = LocalizationManager.Instance.GetText(obj.DescriptionKey);

                    if (isCompleted)
                    {
                        // Đã xong (thường dùng cho Parallel): Gạch ngang + Xám
                        textObj.text = $"<s>{description}</s>";
                        textObj.color = Color.gray;
                    }
                    else
                    {
                        // Chưa xong: Chữ trắng bình thường
                        textObj.text = description;
                        textObj.color = Color.white;
                    }
                }
            }
        }
    }
}