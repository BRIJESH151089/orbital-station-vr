using System;
using UnityEngine;
using Orbital.Core;

namespace Orbital.Activities
{
    /// <summary>Room 4: press the button sequence, then pull the lever.</summary>
    public class CommandDeckPuzzle : PuzzleBase
    {
        [SerializeField] private int[] _sequence = { 2, 0, 3, 1 };

        public SequenceLogic Sequence { get; private set; }
        public LeverLogic Lever { get; private set; }

        public bool SequenceComplete => Sequence != null && Sequence.IsComplete;

        public override string ProgressText =>
            Sequence == null ? string.Empty :
            Sequence.IsComplete ? "Pull the lever" :
            $"Sequence {Sequence.Progress}/{Sequence.Total}";

        /// <summary>Raised when a wrong button is pressed and the sequence resets.</summary>
        public event Action SequenceRejected;

        private void Awake()
        {
            Sequence = new SequenceLogic(_sequence);
            Lever = new LeverLogic();
            Sequence.Changed += RaiseProgressChanged;
            Sequence.Rejected += OnSequenceRejected;
            Lever.Pulled += MarkSolved;
        }

        private void OnSequenceRejected() => SequenceRejected?.Invoke();

        /// <summary>Hook to a button's select event with that button's id.</summary>
        public void PressButton(int id)
        {
            if (State != PuzzleState.Active) return;
            Sequence.Press(id);
        }

        /// <summary>Hook to the lever reader's onChanged event with the normalized 0..1 position.</summary>
        public void SetLever(float normalized)
        {
            if (State != PuzzleState.Active || !Sequence.IsComplete) return;
            Lever.SetPosition(normalized);
        }

        private void OnDestroy()
        {
            if (Sequence != null)
            {
                Sequence.Changed -= RaiseProgressChanged;
                Sequence.Rejected -= OnSequenceRejected;
            }
            if (Lever != null) Lever.Pulled -= MarkSolved;
        }
    }
}
