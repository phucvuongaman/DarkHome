using System.Collections.Generic;
using UnityEngine;

// Vấn đề: Game có nhiều 'loại' ký ức.
// 1. Ký ức vĩnh viễn (Global - thành tựu, cửa bí mật).
// 2. Ký ức "di sản" (Previous - những gì đã làm ở chapter trước).
// 3. Ký ức của chapter hiện tại (Runtime - đã nói chuyện, nhặt chìa khóa).
// Giải pháp: Dùng 3 HashSet riêng biệt.
// Tại sao là HashSet? Vì nó kiểm tra `Contains` (HasFlag) cực nhanh
// và tự động chống trùng lặp. Tốt hơn `List`.

namespace DarkHome
{
    public class FlagManager : MonoBehaviour, IDataPersistence
    {
        public static FlagManager Instance { get; private set; }

        // --- BA TẦNG KÝ ỨC ---
        private HashSet<string> _globalFlags = new HashSet<string>();
        private HashSet<string> _previousChapterFlags = new HashSet<string>();
        private HashSet<string> _runtimeFlags = new HashSet<string>();

        [SerializeField] private FlagDataSO _initialGlobalFlagsSO;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            LoadInitialGlobalFlags();
        }

        private void LoadInitialGlobalFlags()
        {
            if (_initialGlobalFlagsSO != null && _initialGlobalFlagsSO.Flags != null)
            {
                foreach (var flag in _initialGlobalFlagsSO.Flags)
                {
                    if (flag.Scope == EFlagScope.Global)
                        _globalFlags.Add(flag.FlagID);
                }
            }
        }

        #region Chapter Lifecycle
        public void PrepareFlagsForNewChapter(FlagDataSO newChapterOriginFlags, bool isReplaying)
        {
            _runtimeFlags.Clear();
            if (newChapterOriginFlags != null && newChapterOriginFlags.Flags != null)
            {
                foreach (var flag in newChapterOriginFlags.Flags)
                {
                    if (flag.Scope == EFlagScope.Local)
                        _runtimeFlags.Add(flag.FlagID);
                }
            }


            // Nếu KHÔNG phải chơi lại (!isReplaying)...
            if (!isReplaying)
            {
                // Nếu là lần đầu chơi chapter này thì sẽ cộng thêm toàn bộ "di sản" từ chapter trước vào. Khá tuyệt vời nhưng khá dễ quên
                foreach (var prevFlag in _previousChapterFlags)
                {
                    _runtimeFlags.Add(prevFlag);
                }
            }
            // (Nếu `isReplaying = true`, bước 3 bị bỏ qua -> chapter được reset)
        }

        public void CommitRuntimeFlagsToHistory()
        {
            _previousChapterFlags = new HashSet<string>(_runtimeFlags);
        }
        #endregion

        #region Flag Manipulation
        public void AddFlag(FlagData flag)
        {
            if (flag == null || string.IsNullOrEmpty(flag.FlagID)) return;
            HashSet<string> targetSet = (flag.Scope == EFlagScope.Global) ? _globalFlags : _runtimeFlags;

            if (targetSet.Add(flag.FlagID))
            {
                EventManager.Notify(GameEvents.Flag.OnFlagChanged, flag);
                // Debug.Log($"🚩 Flag Added ({flag.Scope}): {flag.FlagID}");
            }
        }

        public void AddFlags(IEnumerable<FlagData> flagList)
        {
            if (flagList == null) return;
            foreach (var f in flagList) AddFlag(f);
        }

        public void RemoveFlag(FlagData flag)
        {
            if (flag == null || string.IsNullOrEmpty(flag.FlagID)) return;
            HashSet<string> targetSet = (flag.Scope == EFlagScope.Global) ? _globalFlags : _runtimeFlags;
            targetSet.Remove(flag.FlagID);
        }

        public void RemoveFlags(IEnumerable<FlagData> flagList)
        {
            if (flagList == null) return;
            foreach (var f in flagList) RemoveFlag(f);
        }
        #endregion

        #region Flag Checking
        public bool HasFlag(FlagData flag)
        {
            if (flag == null || string.IsNullOrEmpty(flag.FlagID)) return false;
            // Kiểm tra cả hai bộ nhớ
            return _runtimeFlags.Contains(flag.FlagID) || _globalFlags.Contains(flag.FlagID);
        }

        public bool HasAllFlags(List<FlagData> requiredFlags)
        {
            if (requiredFlags == null || requiredFlags.Count == 0) return true;
            foreach (var requiredFlag in requiredFlags)
            {
                if (!HasFlag(requiredFlag)) // Dùng lại hàm HasFlag cho gọn
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasAnyFlag(IEnumerable<FlagData> flagList)
        {
            if (flagList == null) return false;
            foreach (var flag in flagList)
            {
                if (HasFlag(flag)) return true;
            }
            return false;
        }
        #endregion

        #region Utility & Save/Load
        public List<string> GetAllRuntimeFlags()
        {
            return new List<string>(_runtimeFlags);
        }

        public List<string> GetAllGlobalFlags()
        {
            return new List<string>(_globalFlags);
        }

        public void SaveData(ref SaveData data)
        {
            data.globalFlags = new List<string>(_globalFlags);
            data.previousChapterFlags = new List<string>(_previousChapterFlags);
            // Lưu runtime flags (tiến độ ngày hiện tại: C1_PROGRESS_DAY2_CLEANED, v.v.)
            data.flags = new List<FlagData>();
            foreach (var f in _runtimeFlags)
                data.flags.Add(new FlagData(f, EFlagScope.Local));
        }

        public void LoadData(SaveData data)
        {
            if (data.globalFlags != null)
                _globalFlags = new HashSet<string>(data.globalFlags);

            if (data.previousChapterFlags != null)
                _previousChapterFlags = new HashSet<string>(data.previousChapterFlags);

            // Khôi phục runtime flags (tiến độ ngày)
            if (data.flags != null)
            {
                foreach (var f in data.flags)
                    _runtimeFlags.Add(f.FlagID);
            }
        }
        #endregion
    }
}
