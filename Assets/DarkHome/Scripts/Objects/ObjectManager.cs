using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class ObjectManager : MonoBehaviour, IDataPersistence
    {
        public static ObjectManager Instance { get; set; }


        private Dictionary<string, BaseObject> _allInteractableObjects = new();
        private Dictionary<string, GameObject> _collectedObjects = new();


        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.AddObserver<ItemDataSO>(GameEvents.Object.OnItemCollected, HandleItemCollected);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<ItemDataSO>(GameEvents.Object.OnItemCollected, HandleItemCollected);
        }

        public void StoreObject(string id, GameObject obj)
        {
            obj.SetActive(false);
            _collectedObjects[id] = obj;
        }


        public void Register(BaseObject obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.Id)) return;
            if (!_allInteractableObjects.ContainsKey(obj.Id))
            {
                _allInteractableObjects.Add(obj.Id, obj);
            }
        }
        public BaseObject GetObjectById(string id)
        {
            _allInteractableObjects.TryGetValue(id, out var obj);
            return obj; // Không cần GetComponent nữa vì Dictionary đã lưu đúng kiểu
        }
        public void Clear()
        {
            _allInteractableObjects.Clear();
            _collectedObjects.Clear();
        }

        /// <summary>
        /// Xóa sạch danh sách vật thể tương tác trong Scene hiện tại.
        /// Gọi hàm này khi chuyển Scene để tránh lỗi "MissingReference".
        /// </summary>
        public void ClearSceneObjects()
        {
            // Chỉ xóa danh sách quản lý tương tác, KHÔNG xóa _collectedObjects (Túi đồ)
            _allInteractableObjects.Clear();
        }

        public List<ObjectData> AllInteractacbleObjectToList()
        {
            List<ObjectData> _allInteractacbleObjectData = new();
            foreach (var item in _allInteractableObjects)
            {
                _allInteractacbleObjectData.Add(item.Value.GetCurrentObjectData());
            }
            return _allInteractacbleObjectData;
        }

        public void LoadObjectData(List<ObjectData> data)
        {
            if (data == null) return;

            foreach (ObjectData item in data)
            {
                _allInteractableObjects[item.objId].LoadData(item);
            }
        }

        // Hàm để kiểm tra xem một item có trong túi đồ không
        public bool IsItemCollected(string itemId)
        {
            return _collectedObjects.ContainsKey(itemId);
        }

        // Hàm để "tiêu thụ" một item sau khi sử dụng
        public void ConsumeItem(string itemId)
        {
            if (_collectedObjects.ContainsKey(itemId))
            {
                // Bạn có thể chỉ xóa khỏi danh sách, hoặc phá hủy GameObject hoàn toàn
                // Destroy(_collectedObjects[itemId]); 
                _collectedObjects.Remove(itemId);
            }
        }



        private void HandleItemCollected(ItemDataSO collectedItemData)
        {
            // Khi nghe thấy item được nhặt, tìm gameobject của nó và cất đi
            var obj = GetObjectById(collectedItemData.itemID);
            if (obj != null)
            {
                StoreObject(obj.Id, obj.gameObject);
            }
        }

        public void SaveData(ref SaveData data)
        {
            // QUAN TRỌNG: Nếu list chưa có (lần đầu), tạo mới. 
            // Nếu đã có dữ liệu cũ (của Scene khác), GIỮ NGUYÊN!
            if (data.allObject == null)
            {
                data.allObject = new List<ObjectData>();
            }

            // Duyệt qua tất cả object đang có trong Scene hiện tại (Ví dụ: Trường)
            foreach (var pair in _allInteractableObjects)
            {
                ObjectData currentData = pair.Value.GetCurrentObjectData();

                // Tìm xem object này đã từng được lưu trong danh sách chung chưa?
                // (Dùng ID để tìm kiếm trong cái túi to)
                int index = data.allObject.FindIndex(x => x.objId == currentData.objId);

                if (index != -1)
                {
                    // NẾU CÓ RỒI: Cập nhật lại trạng thái mới nhất (Ghi đè cái cũ)
                    data.allObject[index] = currentData;
                }
                else
                {
                    // NẾU CHƯA CÓ: Thêm mới vào danh sách
                    data.allObject.Add(currentData);
                }
            }

            // Lưu Inventory 
            // (Vì khi LoadData ta đã nạp lại _collectedObjects, nên ở đây save lại keys là an toàn)
            // data.collectedObjectIDs = _collectedObjects.Keys.ToList();
            if (data.collectedObjectIDs == null) data.collectedObjectIDs = new List<string>();

            // Chỉ thêm những món chưa có trong danh sách save
            foreach (var id in _collectedObjects.Keys)
            {
                if (!data.collectedObjectIDs.Contains(id))
                {
                    data.collectedObjectIDs.Add(id);
                }
            }
        }

        public void LoadData(SaveData data)
        {

            // Khôi phục trạng thái của TẤT CẢ object
            if (data.allObject != null)
            {
                foreach (ObjectData objectData in data.allObject)
                {
                    // Dùng TryGetValue thay vì truy cập trực tiếp index [] để tránh lỗi KeyNotFound
                    if (_allInteractableObjects.TryGetValue(objectData.objId, out BaseObject obj))
                    {
                        // THÊM CHECK NULL Ở ĐÂY:
                        // Unity overload toán tử null, check này sẽ biết object đã bị destroy chưa
                        if (obj != null)
                        {
                            obj.LoadData(objectData);
                        }
                        else
                        {
                            // Object trong list đã chết (do chưa Clear kịp), bỏ qua nó
                        }
                    }
                }
            }

            // "Cất" lại những object đã được thu thập vào kho
            _collectedObjects.Clear(); // Dọn dẹp kho cũ trước
            if (data.collectedObjectIDs != null)
            {
                foreach (string collectedId in data.collectedObjectIDs)
                {
                    if (_allInteractableObjects.TryGetValue(collectedId, out BaseObject objToStore))
                    {
                        StoreObject(collectedId, objToStore.gameObject);
                    }
                }
            }
        }
    }
}