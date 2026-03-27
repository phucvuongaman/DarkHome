/// <summary>
/// Này để check dòng nào đã nói rồi, để lần sau gặp lại NPC thì nó sẽ nói dòng khác
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DarkHome
{
    public class DialogueTrackingState : MonoBehaviour, IDataPersistence
    {
        public static DialogueTrackingState Instance { get; private set; }

        private HashSet<string> _talkedNodes = new HashSet<string>();

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        // Check xem node đã đucợc nói chưa, trả về true nếu đã nói, false nếu chưa
        public bool HasTalkedToNode(string nodeId)
        {
            return _talkedNodes.Contains(nodeId);
        }


        // Đánh dấu node đã được nói
        public void MarkNodeAsTalked(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                _talkedNodes.Add(nodeId);
            }
        }


        public List<string> GetAllTrackingState() => _talkedNodes.ToList();
        public void LoadAllTalkedNodes(List<string> talkedNodes)
        {
            foreach (string nodeId in talkedNodes)
            {
                _talkedNodes.Add(nodeId);
            }
        }

        public void SaveData(ref SaveData data)
        {
            data.talkedNodes = GetAllTrackingState();
        }

        public void LoadData(SaveData data)
        {
            LoadAllTalkedNodes(data.talkedNodes);
        }
    }

    // [System.Serializable]
    // [CreateAssetMenu(fileName = "DialogueTrackingStateSO", menuName = "SO/Dialogue/Dialogue Tracking State")]
    // public class DialogueTrackingStateSO : ScriptableObject
    // {
    //     public List<string> talkedNodes;
    // }
}