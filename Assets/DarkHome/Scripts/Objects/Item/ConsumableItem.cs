using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarkHome
{
    /// <summary>
    /// ConsumableItem kế thừa từ Item.
    /// Các hiệu ứng (Hồi Sanity/Health) được định nghĩa trong ItemDataSO.onPickupEffects.
    /// Không cần override gì thêm vì logic đã có trong Item.cs.
    /// </summary>
    public class ConsumableItem : Item
    {
        // Để trống - Mọi logic đã được xử lý trong Item.cs
        // Nếu muốn thêm logic đặc biệt cho ConsumableItem (VD: animation uống thuốc),
        // override OnInteractPress() ở đây.
    }
}