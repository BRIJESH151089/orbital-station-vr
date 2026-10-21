using System;

namespace Orbital.Core
{
    /// <summary>Tracks progress through a fixed button-press sequence. No Unity dependencies.</summary>
    public class SequenceLogic
    {
        private readonly int[] _expected;

        public int Total => _expected.Length;
        public int Progress { get; private set; }
        public int Mistakes { get; private set; }
        public bool IsComplete => Progress == Total;

        public event Action Changed;
        public event Action Completed;
        public event Action Rejected;

        public SequenceLogic(int[] expected)
        {
            if (expected == null || expected.Length == 0)
                throw new ArgumentException("Need at least one expected id.", nameof(expected));
            _expected = (int[])expected.Clone();
        }

        public void Press(int id)
        {
            if (IsComplete) return;

            if (id == _expected[Progress])
            {
                Progress++;
                Changed?.Invoke();
                if (IsComplete) Completed?.Invoke();
            }
            else
            {
                bool had = Progress > 0;
                Progress = 0;
                Mistakes++;
                Rejected?.Invoke();
                if (had) Changed?.Invoke();
            }
        }

        public void Reset()
        {
            bool had = Progress != 0;
            Progress = 0;
            if (had) Changed?.Invoke();
        }
    }
}
