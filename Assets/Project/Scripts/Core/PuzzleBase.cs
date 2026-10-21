using System;
using UnityEngine;

namespace Orbital.Core
{
    public enum PuzzleState { Locked, Active, Solved }

    /// <summary>Base class for one activity. Holds state and raises events. No XR code.</summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        public PuzzleState State { get; private set; } = PuzzleState.Locked;

        public event Action<PuzzleBase> Solved;
        public event Action<PuzzleBase> HintRequested;
        [SerializeField] private PuzzleInfo _info;

        /// <summary>Text shown on the mission panel. May be null.</summary>
        public PuzzleInfo Info => _info;

        /// <summary>Whether solving this puzzle awards score points. True by default.</summary>
        public virtual bool AwardsPoints => true;

        /// <summary>How many hints have been shown so far (0 = none).</summary>
        public int HintLevel { get; private set; }

        /// <summary>Whether a further hint is available right now.</summary>
        public bool HasHintsLeft => State == PuzzleState.Active && Info != null && Info.HintCount > HintLevel;

        /// <summary>Short progress text such as "Cells placed 1/3". Empty by default.</summary>
        public virtual string ProgressText => string.Empty;

        /// <summary>Raised when ProgressText has changed.</summary>
        public event Action<PuzzleBase> ProgressChanged;

        protected void RaiseProgressChanged() => ProgressChanged?.Invoke(this);

        /// <summary>Unlocks the puzzle so the player can start it.</summary>
        public void Activate()
        {
            if (State != PuzzleState.Locked) return;
            State = PuzzleState.Active;
            OnActivated();
        }

        /// <summary>Called by a subclass when the player completes the activity.</summary>
        protected void MarkSolved()
        {
            if (State != PuzzleState.Active) return;
            State = PuzzleState.Solved;
            Solved?.Invoke(this);
        }

        public void ShowHint()
        {
            if (State != PuzzleState.Active) return;

            int max = Info != null ? Info.HintCount : -1;
            if (max == 0) return;
            if (max > 0 && HintLevel >= max) return;

            HintLevel++;
            HintRequested?.Invoke(this);
        }

        protected virtual void OnActivated() { }
    }
}