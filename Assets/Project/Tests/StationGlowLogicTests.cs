using NUnit.Framework;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class StationGlowLogicTests
    {
        [Test]
        public void TargetIntensity_AtZeroFilled_IsDimFloor()
        {
            Assert.AreEqual(StationGlowLogic.DimIntensity, StationGlowLogic.TargetIntensity(0, 3));
        }

        [Test]
        public void TargetIntensity_AllFilled_IsFull()
        {
            Assert.AreEqual(StationGlowLogic.FullIntensity, StationGlowLogic.TargetIntensity(3, 3));
        }

        [Test]
        public void TargetIntensity_PartialValues_InterpolateBetweenFloorAndFull()
        {
            float oneThird = StationGlowLogic.TargetIntensity(1, 3);
            float twoThirds = StationGlowLogic.TargetIntensity(2, 3);

            Assert.Greater(oneThird, StationGlowLogic.DimIntensity);
            Assert.Less(oneThird, twoThirds);
            Assert.Less(twoThirds, StationGlowLogic.FullIntensity);
        }

        [Test]
        public void TargetIntensity_TotalOfZero_IsSafeAndReturnsFloor()
        {
            Assert.AreEqual(StationGlowLogic.DimIntensity, StationGlowLogic.TargetIntensity(0, 0));
            Assert.AreEqual(StationGlowLogic.DimIntensity, StationGlowLogic.TargetIntensity(5, -1));
        }

        [Test]
        public void Advance_TowardHigherTarget_IncreasesWithoutOvershooting()
        {
            var glow = new StationGlowLogic(fadeSeconds: 1f);
            float target = StationGlowLogic.TargetIntensity(3, 3);

            float intensity = glow.CurrentIntensity;
            for (int i = 0; i < 10; i++)
            {
                float next = glow.Advance(3, 3, deltaTime: 0.1f);
                Assert.GreaterOrEqual(next, intensity);
                Assert.LessOrEqual(next, target);
                intensity = next;
            }
        }

        [Test]
        public void Advance_RemovingACell_LowersIntensity()
        {
            var glow = new StationGlowLogic(fadeSeconds: 1f);
            for (int i = 0; i < 20; i++) glow.Advance(3, 3, deltaTime: 0.1f);
            float fullyLit = glow.CurrentIntensity;

            float afterRemoval = glow.Advance(2, 3, deltaTime: 0.1f);

            Assert.Less(afterRemoval, fullyLit);
        }

        [Test]
        public void Advance_LargeDeltaTime_SnapsToTargetWithoutOvershooting()
        {
            var glow = new StationGlowLogic(fadeSeconds: 1f);
            float target = StationGlowLogic.TargetIntensity(3, 3);

            float result = glow.Advance(3, 3, deltaTime: 1000f);

            Assert.AreEqual(target, result);
        }

        [Test]
        public void Advance_ZeroOrNegativeDeltaTime_DoesNotChangeIntensity()
        {
            var glow = new StationGlowLogic(fadeSeconds: 1f);
            float before = glow.CurrentIntensity;

            Assert.AreEqual(before, glow.Advance(3, 3, deltaTime: 0f));
            Assert.AreEqual(before, glow.Advance(3, 3, deltaTime: -1f));
        }
    }
}
