using System;

namespace Orbital.Core
{
    // Tracks which bin each item sits in and whether every item is in its correct bin.
    public class SortingLogic
    {
        private readonly int[] _correctBin;   // required bin per item
        private readonly int[] _currentBin;   // -1 = not in any bin

        public int Total => _correctBin.Length;
        public int CorrectCount { get; private set; }
        public int Mistakes { get; private set; }
        public bool IsComplete => CorrectCount == Total;

        public event Action Changed;
        public event Action Completed;
        public event Action Mistake;

        public SortingLogic(int[] correctBinPerItem)
        {
            if (correctBinPerItem == null || correctBinPerItem.Length == 0)
                throw new ArgumentException("Need at least one item.", nameof(correctBinPerItem));
            _correctBin = (int[])correctBinPerItem.Clone();
            _currentBin = new int[_correctBin.Length];
            for (int i = 0; i < _currentBin.Length; i++) _currentBin[i] = -1;
        }

        public void Place(int item, int bin)
        {
            if (item < 0 || item >= Total || _currentBin[item] == bin) return;

            bool wasCorrect = _currentBin[item] == _correctBin[item];
            _currentBin[item] = bin;
            bool isCorrect = bin == _correctBin[item];

            if (wasCorrect) CorrectCount--;
            if (isCorrect) CorrectCount++;
            else { Mistakes++; Mistake?.Invoke(); }

            Changed?.Invoke();
            if (IsComplete) Completed?.Invoke();
        }

        public void Remove(int item)
        {
            if (item < 0 || item >= Total || _currentBin[item] == -1) return;
            if (_currentBin[item] == _correctBin[item]) CorrectCount--;
            _currentBin[item] = -1;
            Changed?.Invoke();
        }
    }
}