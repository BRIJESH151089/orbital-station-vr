using System;

namespace Orbital.Core
{
    /// <summary>Tracks a lever's pulled/released state from a normalized 0..1 position. No Unity dependencies.</summary>
    public class LeverLogic
    {
        private readonly float _pullThreshold;
        private readonly float _releaseThreshold;

        public bool IsPulled { get; private set; }

        public event Action Pulled;
        public event Action Released;

        public LeverLogic(float pullThreshold = 0.9f, float releaseThreshold = 0.2f)
        {
            if (!(releaseThreshold >= 0f && releaseThreshold < pullThreshold && pullThreshold <= 1f))
                throw new ArgumentException("Thresholds must satisfy 0 <= release < pull <= 1.");
            _pullThreshold = pullThreshold;
            _releaseThreshold = releaseThreshold;
        }

        public void SetPosition(float v)
        {
            v = Math.Max(0f, Math.Min(1f, v));

            if (!IsPulled && v >= _pullThreshold)
            {
                IsPulled = true;
                Pulled?.Invoke();
            }
            else if (IsPulled && v <= _releaseThreshold)
            {
                IsPulled = false;
                Released?.Invoke();
            }
        }
    }
}
