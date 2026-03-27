using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DarkHome
{
    public class PuzzleCodeInput : PuzzleBase
    {
        [SerializeField] private TMP_Text displayText;

        private string currentCode = "";
        private bool isSolved = false;

        public void PressDigit(string digit)
        {
            if (isSolved) return;

            if (digit == "Enter")
            {
                if (CheckSolved())
                {
                    TrySolve();
                    isSolved = true;
                }
                else
                {
                    Debug.Log("Mã sai.");
                }
            }
            else if (digit == "Clear")
            {
                currentCode = "";
            }
            else
            {
                currentCode += digit;
                if (currentCode.Length > 6)
                    currentCode = currentCode.Substring(0, 6);
            }

            UpdateDisplay();
        }

        public override bool CheckSolved()
        {
            return currentCode == puzzleData.CorrectCode;
        }

        private void UpdateDisplay()
        {
            if (displayText != null)
                displayText.text = currentCode;
        }


    }

}