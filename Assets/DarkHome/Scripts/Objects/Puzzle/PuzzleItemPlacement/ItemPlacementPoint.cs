using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [Serializable]
    // public class ItemPlacementPoint : BaseInteractable
    // {
    //     public Item itemData; // chứa prefab
    //     public bool hasPlacedCorrectItem = false;
    //     public override InteractableType InteractType { get; set; } = InteractableType.PuzzlePoint;

    //     public void TryPlace()
    //     {
    //         if (hasPlacedCorrectItem) return;
    //         if (itemData != null)
    //         {
    //             if (FlagManager.Instance.HasAllFlags(itemData.RequiredFlags))
    //             {
    //                 hasPlacedCorrectItem = true;
    //                 Instantiate(itemData.gameObject, transform.position, transform.rotation);
    //                 // itemData.enabled = false;
    //             }
    //             else
    //             {
    //                 Debug.LogWarning("ItemData rỗng, không thể đặt");
    //             }
    //         }

    //     }


    //     public override void OnInteractPress(Interactor interactor)
    //     {
    //         TryPlace();
    //     }

    //     private void Start()
    //     {
    //         if (hasPlacedCorrectItem && FlagManager.Instance.HasAllFlags(itemData.RequiredFlags))
    //         {
    //             Instantiate(itemData.gameObject, transform.position, transform.rotation);
    //         }
    //     }


    // }

    public class ItemPlacementPoint : BaseObject
    {
        [Tooltip("Kéo GameObject 'Bản đầy đủ' đã được tắt sẵn vào đây")]
        [SerializeField] private GameObject _fullItemObject;

        public bool hasPlacedCorrectItem = false;


        public override void OnInteractPress(Interactor interactor)
        {
            if (hasPlacedCorrectItem) return;
            // Kiểm tra xem người chơi có item cần thiết trong túi đồ không
            // Debug.Log($"id {Id} " + ObjectManager.Instance.IsItemCollected(Id));
            if (ObjectManager.Instance.IsItemCollected(Id))
            {
                // Nếu có, hiện "Bản đầy đủ" lên
                if (_fullItemObject != null)
                {
                    _fullItemObject.SetActive(true);
                }

                // Tiêu thụ item
                ObjectManager.Instance.ConsumeItem(Id);

                // Ẩn "Bóng mờ" đi
                gameObject.SetActive(false);
                hasPlacedCorrectItem = true;
                // Thông báo cho Puzzle cha biết
                // Debug.Log("hasPlacedCorrectItem " + hasPlacedCorrectItem);

                // TODO: Thêm hiệu ứng hay event âm thanh các thứ vào đây

            }
            else
            {
                // TODO: Thêm hiệu ứng hay event chưa tìm thấy vào đây
                // Phát âm thanh "kẹt" hoặc hiển thị thông báo "Cần item..."
            }
        }
    }
}