using NUnit.Framework;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class SocketGlowLogicTests
    {
        [Test]
        public void TargetIntensity_Filled_IsFull()
        {
            Assert.AreEqual(StationGlowLogic.FullIntensity, SocketGlowLogic.TargetIntensity(filled: true));
        }

        [Test]
        public void TargetIntensity_Empty_IsDimFloor()
        {
            Assert.AreEqual(StationGlowLogic.DimIntensity, SocketGlowLogic.TargetIntensity(filled: false));
        }

        [Test]
        public void Advance_TowardFilled_IncreasesWithoutOvershooting()
        {
            var glow = new SocketGlowLogic(fadeSeconds: 0.5f);

            float intensity = glow.CurrentIntensity;
            for (int i = 0; i < 10; i++)
            {
                float next = glow.Advance(filled: true, deltaTime: 0.05f);
                Assert.GreaterOrEqual(next, intensity);
                Assert.LessOrEqual(next, StationGlowLogic.FullIntensity);
                intensity = next;
            }
        }

        [Test]
        public void Advance_RemovingTheCell_FadesDown()
        {
            var glow = new SocketGlowLogic(fadeSeconds: 0.5f);
            for (int i = 0; i < 20; i++) glow.Advance(filled: true, deltaTime: 0.05f);
            float fullyLit = glow.CurrentIntensity;

            float afterRemoval = glow.Advance(filled: false, deltaTime: 0.05f);

            Assert.Less(afterRemoval, fullyLit);
        }

        [Test]
        public void Advance_LargeDeltaTime_SnapsToTargetWithoutOvershooting()
        {
            var glow = new SocketGlowLogic(fadeSeconds: 0.5f);

            float result = glow.Advance(filled: true, deltaTime: 1000f);

            Assert.AreEqual(StationGlowLogic.FullIntensity, result);
        }

        [Test]
        public void Advance_ZeroOrNegativeDeltaTime_DoesNotChangeIntensity()
        {
            var glow = new SocketGlowLogic(fadeSeconds: 0.5f);
            float before = glow.CurrentIntensity;

            Assert.AreEqual(before, glow.Advance(filled: true, deltaTime: 0f));
            Assert.AreEqual(before, glow.Advance(filled: true, deltaTime: -1f));
        }

        [Test]
        public void NewSocketGlow_StartsAtDimFloor()
        {
            var glow = new SocketGlowLogic();

            Assert.AreEqual(StationGlowLogic.DimIntensity, glow.CurrentIntensity);
        }
    }
}
