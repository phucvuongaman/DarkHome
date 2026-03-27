using UnityEngine;

namespace DarkHome
{
    [CreateAssetMenu(fileName = "ChapterData_New", menuName = "SO/Chapter Data")]
    public class ChapterDataSO : ScriptableObject
    {
        [Header("Chapter Info")]
        public string chapterId;
        public string chapterName;

        [Header("Chapter Data")]
        public QuestDataSO defaultQuests;
        public FlagDataSO originFlags;
        public string sceneName;
        public string startSpawnID;

        [Header("UI (Chapter Selection)")]
        [Tooltip("Số thứ tự chapter (1, 2, 3...) hiển thị trong UI")]
        public int chapterNumber = 1;

        [Tooltip("Mô tả ngắn cho Chapter Selection preview panel")]
        [TextArea(3, 5)]
        public string chapterDescription = "";

        [Tooltip("Ảnh preview hiển thị khi select chapter")]
        public Sprite previewSprite;

        [Header("Progression")]
        public ChapterDataSO nextChapterData; // Tham chiếu đến chapter tiếp theo
    }
}