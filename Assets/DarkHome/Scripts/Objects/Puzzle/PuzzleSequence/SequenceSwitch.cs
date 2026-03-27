using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    [RequireComponent(typeof(OutLineController))]
    [Serializable]
    public class SequenceSwitch : BaseInteractable
    {
        public string switchID;
        public PuzzleSequence puzzle;
        protected OutLineController outLine;

        public override InteractableType InteractType { get; set; } = InteractableType.PuzzleSwitch;

        protected virtual void Awake()
        {
            outLine = GetComponent<OutLineController>();
        }
        public override void OnInteractPress(Interactor interactor)
        {
            puzzle?.RegisterPress(switchID);
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