using System;

namespace Orbital.Core
{
    /// <summary>Smooths one socket's indicator plate between dim (empty) and full (filled) brightness. No Unity dependencies.</summary>
    public class SocketGlowLogic
    {
        private const float DefaultFadeSeconds = 0.5f;
        private const float SnapEpsilon = 0.0001f;

        private readonly float _fadeSeconds;

        public float CurrentIntensity { get; private set; }

        public SocketGlowLogic(float fadeSeconds = DefaultFadeSeconds)
        {
            _fadeSeconds = fadeSeconds > 0f ? fadeSeconds : DefaultFadeSeconds;
            CurrentIntensity = StationGlowLogic.DimIntensity;
        }

        /// <summary>The intensity a plate should settle at: full when its socket is filled, dim floor otherwise.</summary>
        public static float TargetIntensity(bool filled) =>
            filled ? StationGlowLogic.FullIntensity : StationGlowLogic.DimIntensity;

        /// <summary>Advances CurrentIntensity toward TargetIntensity(filled) by deltaTime seconds, never overshooting. Returns the new intensity.</summary>
        public float Advance(bool filled, float deltaTime)
        {
            float target = TargetIntensity(filled);
            if (deltaTime <= 0f) return CurrentIntensity;

            float t = Clamp01(deltaTime / _fadeSeconds);
            CurrentIntensity += (target - CurrentIntensity) * t;

            if (Math.Abs(CurrentIntensity - target) < SnapEpsilon) CurrentIntensity = target;

            return CurrentIntensity;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : (value > 1f ? 1f : value);
    }
}
