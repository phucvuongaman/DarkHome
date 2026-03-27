using DarkHome;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DarkHome
{
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _choiceText;
        [SerializeField] private Button _button;
        private Choice _choice;

        public TextMeshProUGUI ChoiceText { get => _choiceText; set => _choiceText = value; }
        public Choice Choice { get => _choice; set => _choice = value; }

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }


        public void SetChoice(Choice choice)
        {
            Choice = choice;
            // Use GetText() for localization support
            // Falls back to ChoiceText for legacy SOs
            ChoiceText.text = choice.GetText();
        }
        public void OnClick()
        {
            if (Choice != null)
            {
                EventManager.Notify(GameEvents.DiaLog.OnChoiceSelected, Choice);
            }
        }

    }
}
