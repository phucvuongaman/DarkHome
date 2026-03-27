using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class PuzzleSequence : PuzzleBase
    {
        private List<string> currentInput = new();

        private bool isSolved = false;

        public void RegisterPress(string switchID)
        {
            if (isSolved) return;

            currentInput.Add(switchID);

            int index = currentInput.Count - 1;

            // Kiểm tra có đúng tới bước này không
            if (puzzleData.CorrectSequence[index] != switchID)
            {
                Debug.Log("Sai thứ tự, reset");
                currentInput.Clear();
                return;
            }

            // Nếu đúng toàn bộ
            if (currentInput.Count == puzzleData.CorrectSequence.Count)
            {
                TrySolve();
                isSolved = true;
            }
        }

        public override bool CheckSolved()
        {
            return currentInput.SequenceEqual(puzzleData.CorrectSequence);
        }


    }

}