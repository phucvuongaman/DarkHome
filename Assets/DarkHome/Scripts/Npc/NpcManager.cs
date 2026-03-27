// Là tổ trưởng khu phố
// ĐI ĐIỀU TRA ĐÂN SỐ
// ĐỂ SAVE LOAD GAME.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DarkHome
{
    public class NpcManager : MonoBehaviour, IDataPersistence
    {
        public static NpcManager Instance { get; private set; }
        private Dictionary<string, NpcContext> _allNpc = new Dictionary<string, NpcContext>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Làm quả giấy khai sinh ở hàm Start NpcContext
        public void Register(NpcContext npcContext)
        {
            if (!_allNpc.ContainsKey(npcContext.Npc.Id))
                _allNpc.Add(npcContext.Npc.Id, npcContext);
        }


        public NpcContext GetNpcById(string id)
        {
            _allNpc.TryGetValue(id, out var npc);
            return npc;
        }

        public void Clear()
        {
            _allNpc.Clear();
        }



        public void SaveData(ref SaveData data)
        {
            // Nếu chưa có list thì tạo mới, có rồi thì GIỮ NGUYÊN để bảo lưu data map khác
            if (data.allNpc == null)
            {
                data.allNpc = new List<NpcData>();
            }

            // Duyệt qua các NPC đang hoạt động ở Scene hiện tại
            foreach (var npcPair in _allNpc)
            {
                // NPC có thể bị hủy đột ngột, kiểm tra cho chắc
                if (npcPair.Value == null) continue;

                NpcData currentData = npcPair.Value.GetCurrentNpcData();

                // Tìm xem NPC này đã có trong danh sách tổng chưa
                int index = data.allNpc.FindIndex(x => x.npcId == currentData.npcId);

                if (index != -1)
                {
                    // Có rồi -> Cập nhật (Ghi đè trạng thái mới nhất)
                    data.allNpc[index] = currentData;
                }
                else
                {
                    // Chưa có -> Thêm mới
                    data.allNpc.Add(currentData);
                }
            }
        }

        public void LoadData(SaveData data)
        {
            // "Người điều tra dân số" đưa "biểu mẫu" đã điền.
            // NpcManager sẽ đọc và ra lệnh cho từng NPC.
            if (data.allNpc == null) return;

            foreach (NpcData npcDataFromFile in data.allNpc)
            {
                // Dùng ID để tìm đúng NPC đang hoạt động trong Dictionary
                if (_allNpc.TryGetValue(npcDataFromFile.npcId, out NpcContext npcToLoad))
                {
                    // Ra lệnh cho NPC đó tự cập nhật trạng thái của nó
                    npcToLoad.LoadData(npcDataFromFile);
                }
                else
                {
                    Debug.LogWarning($"Không tìm thấy NPC với ID '{npcDataFromFile.npcId}' trong scene để load data.");
                }
            }
        }
    }
}