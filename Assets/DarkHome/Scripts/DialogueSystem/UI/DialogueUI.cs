using UnityEngine;
using TMPro;

namespace DarkHome
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TypewriterEffect _typewriter;

        private void Awake()
        {
            // Bắt đầu game thì ẩn đi
            _dialoguePanel.SetActive(false);
        }
        private void OnEnable()
        {
            EventManager.AddObserver<DialogueNode>(GameEvents.DiaLog.ReadDialogue, DisplayDialogue);
            EventManager.AddObserver(GameEvents.DiaLog.EndDialogue, HideDialogue);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener<DialogueNode>(GameEvents.DiaLog.ReadDialogue, DisplayDialogue);
            EventManager.RemoveListener(GameEvents.DiaLog.EndDialogue, HideDialogue);
        }



        private void DisplayDialogue(DialogueNode node)
        {
            _dialoguePanel.SetActive(true);
            _speakerNameText.text = node.IdSpeaker;

            // Use GetText() for localization support
            // Falls back to DialogueText for legacy SOs
            _typewriter.Run(node.GetText());


            if (node.GrantedFlags != null && node.GrantedFlags.Count > 0)
            {
                FlagManager.Instance.AddFlags(node.GrantedFlags);
            }
        }

        // Hàm này sẽ được UIManager gọi
        private void HideDialogue()
        {
            _dialoguePanel.SetActive(false);
            _speakerNameText.text = string.Empty;
            _dialogueText.text = string.Empty;
        }


    }
}