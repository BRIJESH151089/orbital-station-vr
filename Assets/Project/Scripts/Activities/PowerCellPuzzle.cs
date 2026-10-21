using UnityEngine;
using Orbital.Core;

namespace Orbital.Activities
{
    /// <summary>Room 2: place all power cells into their sockets.</summary>
    public class PowerCellPuzzle : PuzzleBase
    {
        [SerializeField] private int _socketCount = 3;

        public SocketTracker Tracker { get; private set; }

        public override string ProgressText =>
            Tracker == null ? string.Empty : $"Cells placed {Tracker.FilledCount}/{Tracker.Total}";

        private void Awake()
        {
            Tracker = new SocketTracker(_socketCount);
            Tracker.Changed += RaiseProgressChanged;
            Tracker.Completed += MarkSolved;
        }

        /// <summary>Hook to a socket's "cell snapped" event with that socket's index.</summary>
        public void SocketFilled(int index) => Tracker.Fill(index);

        /// <summary>Hook to a socket's "cell removed" event with that socket's index.</summary>
        public void SocketEmptied(int index) => Tracker.Empty(index);

        protected override void OnActivated()
        {
            // The player may have placed cells before this room became active.
            if (Tracker.IsComplete) MarkSolved();
        }
    }
}