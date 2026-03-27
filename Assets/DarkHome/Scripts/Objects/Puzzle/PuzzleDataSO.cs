using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public enum EPuzzleType { Code, Sequence, ItemPlacement }

    [CreateAssetMenu(menuName = "SO/Object/PuzzleSO")]
    public class PuzzleDataSO : ScriptableObject
    {
        [Tooltip("ID định danh cho Puzzle (VD: 'PUZZLE_SAFE_01'). Dùng để lưu trạng thái đã giải.")]
        public string Id;

        [Tooltip("Loại câu đố.")]
        public EPuzzleType PuzzleType;

        [Header("Conditions & Rewards")]
        [Tooltip("Cờ yêu cầu để BẮT ĐẦU giải đố (VD: Phải có 'FLAG_READ_NOTE' mới được nhập mã).")]
        public List<FlagData> RequiredFlags;

        [Tooltip("Cờ nhận được khi GIẢI XONG (VD: 'FLAG_SAFE_OPENED').")]
        public List<FlagData> GrantedFlags;

        [Header("Specific Settings")]
        [Tooltip("Dùng cho Puzzle Code: Đáp án đúng (VD: '1234').")]
        public string CorrectCode;

        [Tooltip("Dùng cho Puzzle Sequence: Thứ tự đúng của các công tắc (VD: 'Switch_Left', 'Switch_Right', 'Switch_Mid').")]
        public List<string> CorrectSequence;
    }

}