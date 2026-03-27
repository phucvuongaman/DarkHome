using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    public class DialogueRepository
    {
        private List<DialogueDataSO> _dialogueDataSources;

        public int GetDataSourceCount() => _dialogueDataSources?.Count ?? 0;
        public DialogueRepository(List<DialogueDataSO> dataSources)
        {
            _dialogueDataSources = dataSources;
        }


        // Hàm này sẽ lấy node đầu tiên chưa được nói của speaker
        // Nếu không có, nó sẽ lấy đoạn mặc định
        public DialogueNode GetFirstTalkableOrDefault(string idSpeaker)
        {
            if (_dialogueDataSources == null) return null;

            // Tìm file SO tương ứng với NPC này
            var specificNpcData = _dialogueDataSources.FirstOrDefault(data => data.NpcId == idSpeaker);
            if (specificNpcData == null)
            {
                Debug.LogWarning($"Không tìm thấy DialogueDataSO nào cho NpcId: {idSpeaker}");
                return null;
            }

            // --- BƯỚC 1: KIỂM TRA "LẦN ĐẦU GẶP GỠ" ---
            var startNode = specificNpcData.Nodes.FirstOrDefault(node => node.IsStartNode);
            if (startNode != null && !DialogueTrackingState.Instance.HasTalkedToNode(startNode.NodeId))
            {
                // Nếu có start node và chưa nói, trả về ngay lập tức!
                return startNode;
            }

            // --- BƯỚC 2: TÌM KIẾM THEO NGỮ CẢNH (PRIORITY & FLAGS) ---
            List<DialogueNode> validNodes = new List<DialogueNode>();
            foreach (var node in specificNpcData.Nodes)
            {
                // Bỏ qua nếu là node không lặp lại và đã nói rồi
                if (!node.IsRepeatable && DialogueTrackingState.Instance.HasTalkedToNode(node.NodeId))
                {
                    continue;
                }

                // Kiểm tra xem người chơi có đủ flag yêu cầu không
                if (FlagManager.Instance.HasAllFlags(node.RequiredFlags))
                {
                    validNodes.Add(node);
                }
            }

            // Nếu không có node nào hợp lệ, trả về null (không có gì để nói)
            if (validNodes.Count == 0)
            {
                return null;
            }

            // Sắp xếp danh sách các node hợp lệ theo Priority từ cao xuống thấp
            var sortedNodes = validNodes.OrderByDescending(node => node.Priority);

            // Trả về node có Priority cao nhất
            return sortedNodes.FirstOrDefault();
        }

        public DialogueNode GetNodeById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId) || _dialogueDataSources == null) return null;

            // Quét qua TẤT CẢ các file SO để tìm node có ID khớp
            foreach (var dataSource in _dialogueDataSources)
            {
                var foundNode = dataSource.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            Debug.LogWarning($"Không tìm thấy node nào với ID: {nodeId} trong tất cả các file SO đã tải.");
            return null;
        }
    }
}
