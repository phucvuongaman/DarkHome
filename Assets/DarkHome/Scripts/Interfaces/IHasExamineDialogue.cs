using UnityEngine;

namespace DarkHome
{
    /// <summary>
    /// Interface cho các ObjectDataSO có dialogue khi player examine.
    /// VD: TV, Sofa, Shelf → Trigger player internal monologue
    /// </summary>
    public interface IHasExamineDialogue
    {
        /// <summary>
        /// Lấy TextKey của dialogue suy nghĩ khi examine object.
        /// Return empty string nếu object không cần examine dialogue.
        /// </summary>
        string GetExamineDialogueKey();
    }
}
