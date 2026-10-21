using System;

namespace Orbital.Core
{
    /// <summary>Counts which of N sockets are filled. No Unity dependencies.</summary>
    public class SocketTracker
    {
        private readonly bool[] _filled;

        public int Total => _filled.Length;
        public int FilledCount { get; private set; }
        public bool IsComplete => FilledCount == Total;

        public event Action Changed;
        public event Action Completed;

        public SocketTracker(int total)
        {
            if (total <= 0) throw new ArgumentOutOfRangeException(nameof(total));
            _filled = new bool[total];
        }

        public void Fill(int index)
        {
            if (!InRange(index) || _filled[index]) return;
            _filled[index] = true;
            FilledCount++;
            Changed?.Invoke();
            if (IsComplete) Completed?.Invoke();
        }

        public void Empty(int index)
        {
            if (!InRange(index) || !_filled[index]) return;
            _filled[index] = false;
            FilledCount--;
            Changed?.Invoke();
        }

        /// <summary>Whether the socket at this index is filled. False for an out-of-range index.</summary>
        public bool IsFilled(int index) => InRange(index) && _filled[index];

        private bool InRange(int index) => index >= 0 && index < _filled.Length;
    }
}