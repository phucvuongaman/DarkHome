using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkHome
{
    // Đầu tiên IDataPersistence nó là một hợp đồng bắt buộc khi gã nào đó muốn save load game I
    // Để có thể save load gã đó sẽ phải thêm các biến riêng biệt
    // Hãy cmt ở trên để biết đống biến đó của gã nào.


    // [System.Serializable] << Chỉ thị (Attribute) NÀY LÀ BẮT BUỘC!
    // Nó báo cho Unity: "Hãy cho phép lớp (class) này
    // được biến đổi thành/từ JSON để lưu xuống file."
    [System.Serializable]
    public class SaveData
    {
        // Player
        public Vector3 playerPosition;
        public Quaternion playerRotation = Quaternion.identity;
        public float playerHealth;
        public float playerSanity;

        // Game State
        public string currentSceneName;
        public string currentChapterId;
        public int currentDay = 1;
        public List<FlagData> flags = new List<FlagData>();
        public List<Quest> quests = new List<Quest>();


        // Tracking
        public List<string> talkedNodes = new List<string>(); // <--- Fix lỗi Dialogue
        public List<NpcData> allNpc = new List<NpcData>();
        public List<ObjectData> allObject = new List<ObjectData>();
        public List<string> collectedObjectIDs = new List<string>();

        // Flag
        public int currentChapterIndex;
        public int highestUnlockedChapter = 0; // 0 = Chapter 1 only, 1 = Chapters 1-2, etc.
        public List<string> globalFlags = new List<string>();
        public List<string> previousChapterFlags = new List<string>();
    }
}