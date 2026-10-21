using System;

namespace Orbital.Core
{
    /// <summary>Maps filled/total sockets to a target glow intensity and smooths toward it over time. No Unity dependencies.</summary>
    public class StationGlowLogic
    {
        public const float DimIntensity = 0.15f;
        public const float FullIntensity = 1f;
        private const float DefaultFadeSeconds = 1f;
        private const float SnapEpsilon = 0.0001f;

        private readonly float _fadeSeconds;

        public float CurrentIntensity { get; private set; }

        public StationGlowLogic(float fadeSeconds = DefaultFadeSeconds)
        {
            _fadeSeconds = fadeSeconds > 0f ? fadeSeconds : DefaultFadeSeconds;
            CurrentIntensity = DimIntensity;
        }

        /// <summary>The intensity a station with this many of its sockets filled should settle at. Safe when total is 0 or negative.</summary>
        public static float TargetIntensity(int filled, int total)
        {
            if (total <= 0) return DimIntensity;

            float t = Clamp01((float)filled / total);
            return DimIntensity + (FullIntensity - DimIntensity) * t;
        }

        /// <summary>Advances CurrentIntensity toward TargetIntensity(filled, total) by deltaTime seconds, never overshooting. Returns the new intensity.</summary>
        public float Advance(int filled, int total, float deltaTime)
        {
            float target = TargetIntensity(filled, total);
            if (deltaTime <= 0f) return CurrentIntensity;

            float t = Clamp01(deltaTime / _fadeSeconds);
            CurrentIntensity += (target - CurrentIntensity) * t;

            if (Math.Abs(CurrentIntensity - target) < SnapEpsilon) CurrentIntensity = target;

            return CurrentIntensity;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);
    }
}
