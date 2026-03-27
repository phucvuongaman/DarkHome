// using System.Collections; // Nhớ thêm cái này

// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI; // Thêm thư viện UI

// namespace DarkHome
// {
//     public class ChoiceUI : MonoBehaviour
//     {
//         // KHÔNG CÒN SINGLETON
//         [SerializeField] private GameObject _choicePanel;
//         [SerializeField] private RectTransform _choiceContainer; // Kéo cái "Content" trong ScrollView vào đây cho chắc
//         public bool IsInChoising { get; private set; } = false;

//         private void Start()
//         {
//             _choicePanel.SetActive(false);
//         }

//         // Hiển thị danh sách choice ra UI
//         public void DisplayChoices(List<Choice> validChoices)
//         {
//             if (validChoices == null || validChoices.Count == 0)
//             {
//                 EndChoice();
//                 return;
//             }

//             // Dọn dẹp các choice cũ
//             EventManager.Notify(GameEvents.ObjectPool.HideAll, "Choice");

//             _choicePanel.SetActive(true);
//             IsInChoising = true;

//             // Chỉ việc hiển thị những gì được đưa, không cần logic
//             foreach (var choice in validChoices)
//             {
//                 var go = PoolManager.Instance.GetObjectFromPool("Choice");
//                 if (go != null)
//                 {
//                     // Reset lại scale để tránh bị biến dạng khi lấy từ pool
//                     go.transform.localScale = Vector3.one;

//                     var btn = go.GetComponent<ChoiceButton>();
//                     btn.SetChoice(choice);
//                     go.SetActive(true);

//                     // Đảm bảo nó nằm cuối danh sách để hiển thị đúng thứ tự
//                     go.transform.SetAsLastSibling();
//                 }
//             }

//             StartCoroutine(ForceUpdateLayout());
//         }

//         private IEnumerator ForceUpdateLayout()
//         {
//             // Chờ cuối frame để PoolManager sắp xếp xong xuôi
//             yield return new WaitForEndOfFrame();
//             if (_choiceContainer != null)
//             {
//                 LayoutRebuilder.ForceRebuildLayoutImmediate(_choiceContainer);
//             }
//         }
//         public void EndChoice()
//         {
//             _choicePanel.SetActive(false);
//             IsInChoising = false;
//             EventManager.Notify(GameEvents.ObjectPool.HideAll, "Choice");
//         }

//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkHome
{
    public class ChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private RectTransform _choiceContainer; // Kéo object Content vào đây
        public bool IsInChoising { get; private set; } = false;

        private void Start() => _choicePanel.SetActive(false);

        public void DisplayChoices(List<Choice> validChoices)
        {
            if (validChoices == null || validChoices.Count == 0)
            {
                EndChoice();
                return;
            }

            // 1. Dọn dẹp (Cất hết vào kho)
            EventManager.Notify(GameEvents.ObjectPool.HideAll, "Choice");

            _choicePanel.SetActive(true);
            IsInChoising = true;

            // 2. --- KEY TRICK: DUYỆT NGƯỢC DANH SÁCH ---
            // Duyệt từ cuối về đầu (3 -> 2 -> 1)
            // Kết hợp với PoolManager đang dùng SetAsFirstSibling
            // Thứ tự hiển thị sẽ thành xuôi (1 -> 2 -> 3)
            for (int i = validChoices.Count - 1; i >= 0; i--)
            {
                var choice = validChoices[i];

                var go = PoolManager.Instance.GetObjectFromPool("Choice");
                if (go != null)
                {
                    // Nếu bạn chưa sửa PoolManager thành SetAsFirstSibling thì gọi ở đây luôn cho chắc
                    go.transform.SetAsFirstSibling();

                    var btn = go.GetComponent<ChoiceButton>();
                    btn.SetChoice(choice);
                    go.SetActive(true);
                }
            }

            // 3. Ép Layout cập nhật
            StartCoroutine(ForceUpdateLayout());
        }

        // public void DisplayChoices(List<Choice> validChoices)
        // {
        //     if (validChoices == null || validChoices.Count == 0)
        //     {
        //         EndChoice();
        //         return;
        //     }

        //     // 1. Reset bàn cờ: Cất hết nút cũ vào Stack
        //     EventManager.Notify(GameEvents.ObjectPool.HideAll, "Choice");

        //     _choicePanel.SetActive(true);
        //     IsInChoising = true;

        //     // 2. Lần lượt lấy nút mới ra
        //     // Nhờ lệnh SetAsLastSibling() trong PoolManager,
        //     // nút lấy ra sau sẽ nằm dưới nút lấy ra trước -> thứ tự luôn đúng.
        //     foreach (var choice in validChoices)
        //     {
        //         var go = PoolManager.Instance.GetObjectFromPool("Choice");
        //         if (go != null)
        //         {
        //             go.transform.SetAsLastSibling();

        //             var btn = go.GetComponent<ChoiceButton>();
        //             btn.SetChoice(choice);
        //             go.SetActive(true);
        //         }
        //     }

        //     // 3. Ép Layout cập nhật (Chốt chặn cuối cùng)
        //     StartCoroutine(ForceUpdateLayout());
        // }

        private IEnumerator ForceUpdateLayout()
        {
            // Chờ 1 frame để Unity bật tắt active xong xuôi
            yield return null;

            if (_choiceContainer != null)
            {
                // Ép tính toán lại vị trí ngay lập tức
                LayoutRebuilder.ForceRebuildLayoutImmediate(_choiceContainer);
            }
        }

        public void EndChoice()
        {
            _choicePanel.SetActive(false);
            IsInChoising = false;
            EventManager.Notify(GameEvents.ObjectPool.HideAll, "Choice");
        }
    }
}