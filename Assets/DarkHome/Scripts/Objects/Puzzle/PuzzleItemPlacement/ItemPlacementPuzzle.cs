using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DarkHome
{
    public class ItemPlacementPuzzle : PuzzleBase
    {
        [SerializeField] private List<ItemPlacementPoint> placementPoints;

        public override InteractableType InteractType => InteractableType.Item;

        public override bool CheckSolved()
        {
            // Nếu không có điểm đặt nào được gán, coi như chưa giải được
            if (placementPoints == null || placementPoints.Count == 0)
            {
                return false;
            }
            // Dùng LINQ để kiểm tra xem TẤT CẢ các điểm có `hasPlacedCorrectItem == true` không
            return placementPoints.All(p => p.hasPlacedCorrectItem);
        }

        public override void OnInteractPress(Interactor interactor)
        {


            // Logic đơn giản: Nếu đã giải được thì gọi TrySolve
            if (CheckSolved())
            {
                TrySolve(); // Gọi hàm TrySolve của lớp cha (PuzzleBase)
            }
        }


    }
}