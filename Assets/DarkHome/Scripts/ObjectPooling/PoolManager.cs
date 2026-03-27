
// using System.Collections.Generic;
// using UnityEngine;

// namespace DarkHome
// {
//     public class PoolManager : MonoBehaviour
//     {
//         [SerializeField] private Stack<PoolData> _poolsSetup = new(); // Kéo thả setup trong Inspector
//         private Dictionary<string, PoolData> _pools = new();

//         [SerializeField] private Transform _poolRoot; // Chứa toàn bộ object đã Return để gọn hierarchy

//         public static PoolManager Instance { get; private set; }

//         private void Awake()
//         {
//             if (Instance != null)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             Instance = this;

//             if (_poolRoot == null)
//             {
//                 GameObject rootObj = new GameObject("Pool_Root");
//                 _poolRoot = rootObj.transform;
//                 _poolRoot.SetParent(transform); // để nằm dưới PoolManager
//             }

//             // Khởi tạo Dictionary để tra cứu nhanh
//             foreach (var setup in _poolsSetup)
//             {
//                 if (!_pools.ContainsKey(setup.key))
//                 {
//                     _pools[setup.key] = setup;
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"Pool key '{setup.key}' bị trùng!");
//                 }
//             }
//         }

//         public GameObject GetObjectFromPool(string key)
//         {
//             if (!_pools.ContainsKey(key))
//             {
//                 Debug.LogWarning($"Không tìm thấy pool với key: {key}");
//                 return null;
//             }

//             var poolData = _pools[key];

//             // Nếu còn object sẵn thì lấy ra
//             if (poolData.pool.Count > 0)
//             {
//                 GameObject obj = poolData.pool[0];
//                 poolData.pool.RemoveAt(0);
//                 obj.transform.SetParent(poolData.container, false);

//                 obj.transform.localScale = Vector3.one;
//                 obj.transform.localPosition = Vector3.zero; // Reset vị trí về 0 để Layout Group tự sắp xếp
//                 obj.transform.localRotation = Quaternion.identity;


//                 obj.SetActive(true);
//                 return obj;
//             }

//             // Không có sẵn → tạo mới
//             GameObject newObj = Instantiate(poolData.prefab, poolData.container);
//             newObj.name = poolData.prefab.name; // Tránh thêm "(Clone)"
//             newObj.SetActive(true);
//             return newObj;
//         }

//         public void ReturnToPool(string key, GameObject obj)
//         {
//             if (!_pools.ContainsKey(key))
//             {
//                 Debug.LogWarning($"Không tìm thấy pool với key: {key}");
//                 Destroy(obj); // Nếu không có pool, hủy luôn
//                 return;
//             }

//             obj.SetActive(false);
//             obj.transform.SetParent(_poolRoot, false); // đưa về container pool chung
//             _pools[key].pool.Add(obj);
//         }



//         // Quả này hơi chiến hạm.
//         // Ở hàm này tôi dùng đệ quy với chạy for _pools nên sẽ có trường hợp bên ngoài đang chạy...
//         // ...thì lại bị bên trong đệ quy thêm hay xóa. nên sẽ tạo 1 List toReturn mới để tránh trường hợp trên. 
//         public void HideAll(string key = null)
//         {
//             if (string.IsNullOrEmpty(key))
//             {
//                 // Nếu nó chạy key null thì sẽ lấy toàn bộ GO đang bên ngoài vào kho
//                 // lọc theo key để lại đệ quy thành 1 vòng lặp.
//                 // lần 1 HideAll(null) -> vô if chạy forech -> lần 2 HideAll(k) -> vô ifelse -> nhét GO lại vào pool,...
//                 foreach (var k in _pools.Keys)
//                 {
//                     // vì đang sử dụng chính nó nên sẽ chia ra 2 foreach ở elseif
//                     HideAll(k);
//                 }
//             }
//             else if (_pools.ContainsKey(key))
//             {
//                 var poolData = _pools[key];
//                 // Vì đang foreach chilren trong transform nên không thể thay đổi và tạo một Lít tạm là tuyệt vời
//                 List<Transform> toReturn = new();
//                 // poolData.container nó là một Transform chứa các GO mà đang hiện trên màn hình và
//                 // foreach này để tôi lấy các go đưa vào List
//                 foreach (Transform child in poolData.container) // ĐOẠN NÀY COI LẠI MẤY LẦN RÔI VẪN QUÊN ĐỂ LÀM GÌ
//                 {
//                     toReturn.Add(child);
//                 }

//                 // Xong xuôi thì nhét nó vào pool
//                 foreach (var t in toReturn)
//                 {
//                     ReturnToPool(key, t.gameObject);
//                 }
//             }
//         }

//         private void OnEnable()
//         {
//             EventManager.AddObserver<string>(GameEvents.ObjectPool.HideAll, HideAll);
//         }

//         private void OnDisable()
//         {
//             EventManager.RemoveListener<string>(GameEvents.ObjectPool.HideAll, HideAll);
//         }
//     }

// }


using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] private List<PoolData> _poolsSetup = new();

        // Dùng Stack để quản lý: Nhanh, Gọn, Nhẹ
        private Dictionary<string, Stack<GameObject>> _poolStack = new();
        private Dictionary<string, PoolData> _poolDataLookup = new();

        public static PoolManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            // Khởi tạo Dictionary
            foreach (var setup in _poolsSetup)
            {
                if (!_poolStack.ContainsKey(setup.key))
                {
                    _poolStack.Add(setup.key, new Stack<GameObject>());
                    _poolDataLookup.Add(setup.key, setup);
                }
            }
        }

        public GameObject GetObjectFromPool(string key)
        {
            if (!_poolStack.ContainsKey(key))
            {
                Debug.LogWarning($"Pool key '{key}' không tồn tại!");
                return null;
            }

            GameObject objToSpawn;
            var stack = _poolStack[key];
            var poolData = _poolDataLookup[key];

            // Kiểm tra trong kho (Stack) có hàng không?
            if (stack.Count > 0)
            {
                // Lấy cái trên cùng ra (Pop)
                objToSpawn = stack.Pop();

                // Phòng hờ object bị destroy bất ngờ
                if (objToSpawn == null) return GetObjectFromPool(key);
            }
            else
            {
                // Hết hàng -> Tạo mới (Instantiate)
                objToSpawn = Instantiate(poolData.prefab, poolData.container);
                objToSpawn.name = poolData.prefab.name;
            }

            // Bật lên trước (để các hàm reset hoạt động đúng)
            objToSpawn.SetActive(true);

            // XÓA SẠCH "KÝ ỨC" VỊ TRÍ CŨ
            // Nếu không có đoạn này, nút sẽ nhớ vị trí cũ và đè lên nút khác
            // objToSpawn.transform.SetParent(poolData.container, false);
            // objToSpawn.transform.localPosition = Vector3.zero;
            // objToSpawn.transform.localRotation = Quaternion.identity;
            // objToSpawn.transform.localScale = Vector3.one;

            // Xếp nó xuống cuối hàng -> Đảm bảo thứ tự Choice 1, 2, 3 từ trên xuống dưới
            // objToSpawn.transform.SetAsFirstSibling();

            return objToSpawn;
        }

        public void ReturnToPool(string key, GameObject obj)
        {
            if (!_poolStack.ContainsKey(key))
            {
                Destroy(obj);
                return;
            }

            // Tắt đi
            obj.SetActive(false);
            // Ném trả vào Stack
            _poolStack[key].Push(obj);
        }

        public void HideAll(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                foreach (var k in _poolDataLookup.Keys) HideAll(k);
                return;
            }

            if (_poolDataLookup.TryGetValue(key, out var poolData))
            {
                // Duyệt qua tất cả con trong container
                // Dùng List tạm để tránh lỗi khi thay đổi danh sách lúc đang duyệt
                List<GameObject> activeObjects = new List<GameObject>();

                foreach (Transform child in poolData.container)
                {
                    if (child.gameObject.activeSelf)
                    {
                        activeObjects.Add(child.gameObject);
                    }
                }

                // Cất hết bọn đang active vào kho
                foreach (var obj in activeObjects)
                {
                    ReturnToPool(key, obj);
                }
            }
        }

        private void OnEnable() => EventManager.AddObserver<string>(GameEvents.ObjectPool.HideAll, HideAll);
        private void OnDisable() => EventManager.RemoveListener<string>(GameEvents.ObjectPool.HideAll, HideAll);
    }
}
