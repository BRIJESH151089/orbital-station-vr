using UnityEngine;
using Orbital.Core;

namespace Orbital.Activities
{
    /// <summary>Airlock keypad activity. Buttons call PressDigit(); XR input lives in the Interaction layer.</summary>
    public class KeypadPuzzle : PuzzleBase
    {
        [SerializeField] private string _code = "4827";

        public KeypadLogic Logic { get; private set; }

        private void Awake()
        {
            Logic = new KeypadLogic(_code);
            Logic.Accepted += MarkSolved;
        }

        /// <summary>Hook this to a button's event. Ignored unless the puzzle is Active.</summary>
        public void PressDigit(int digit)
        {
            if (State != PuzzleState.Active) return;
            Logic.Press(digit);
        }

        public void ClearEntry() => Logic.Clear();
    }
}