using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [RequireComponent(typeof(OutLineController))]
    [Serializable]
    public class CodeButton : BaseInteractable
    {
        public string digit; // gán 0 → 9 trong Inspector
        public PuzzleCodeInput puzzle;
        protected OutLineController outLine;

        public override InteractableType InteractType { get; set; } = InteractableType.PuzzleCode;

        protected void Awake()
        {
            outLine = GetComponent<OutLineController>();
        }
        public override void OnInteractPress(Interactor interactor)
        {
            puzzle?.PressDigit(digit);
        }

        public override void OnFocus()
        {
            outLine?.EnableOutline();
        }
        public override void OnLoseFocus()
        {
            outLine?.DisableOutline();
        }
    }

}