using NUnit.Framework;
using UnityEngine;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class PuzzleBaseTests
    {
        // MarkSolved() is protected, so the test subclass exposes it.
        private class TestPuzzle : PuzzleBase
        {
            public void Solve() => MarkSolved();
            public void Report() => RaiseProgressChanged();

        }
        private GameObject _go;
        private TestPuzzle _puzzle;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestPuzzle");
            _puzzle = _go.AddComponent<TestPuzzle>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        private static PuzzleInfo MakeInfo(params string[] hints)
        {
            var info = ScriptableObject.CreateInstance<PuzzleInfo>();
            typeof(PuzzleInfo)
                .GetField("_hints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(info, hints);
            return info;
        }

        private static void SetInfo(PuzzleBase puzzle, PuzzleInfo info)
        {
            typeof(PuzzleBase)
                .GetField("_info", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(puzzle, info);
        }

        [Test]
        public void StartsLocked()
        {
            Assert.AreEqual(PuzzleState.Locked, _puzzle.State);
        }

        [Test]
        public void BecomesActiveAfterActivate()
        {
            _puzzle.Activate();
            Assert.AreEqual(PuzzleState.Active, _puzzle.State);
        }

        [Test]
        public void RaisesSolvedOnceAfterMarkSolved()
        {
            int count = 0;
            _puzzle.Solved += _ => count++;

            _puzzle.Activate();
            _puzzle.Solve();
            _puzzle.Solve(); // second call must be ignored

            Assert.AreEqual(1, count);
            Assert.AreEqual(PuzzleState.Solved, _puzzle.State);
        }

        [Test]
        public void CannotSolveWhileLocked()
        {
            int count = 0;
            _puzzle.Solved += _ => count++;

            _puzzle.Solve(); // never activated

            Assert.AreEqual(0, count);
            Assert.AreEqual(PuzzleState.Locked, _puzzle.State);
        }
        [Test]
        public void ProgressChangedIsRaised()
        {
            int count = 0;
            _puzzle.ProgressChanged += _ => count++;
            _puzzle.Report();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void HintIgnoredWhenNotActive()
        {
            int count = 0;
            _puzzle.HintRequested += _ => count++;

            _puzzle.ShowHint(); // never activated

            Assert.AreEqual(0, count);
            Assert.AreEqual(0, _puzzle.HintLevel);
        }

        [Test]
        public void HintLevelsIncrementUpToHintCount()
        {
            SetInfo(_puzzle, MakeInfo("line1", "line2"));
            _puzzle.Activate();
            int count = 0;
            _puzzle.HintRequested += _ => count++;

            _puzzle.ShowHint();
            Assert.AreEqual(1, _puzzle.HintLevel);
            Assert.AreEqual(1, count);

            _puzzle.ShowHint();
            Assert.AreEqual(2, _puzzle.HintLevel);
            Assert.AreEqual(2, count);
        }

        [Test]
        public void ExhaustedHintsRaiseNoEventAndDoNotChangeLevel()
        {
            SetInfo(_puzzle, MakeInfo("line1"));
            _puzzle.Activate();
            int count = 0;
            _puzzle.HintRequested += _ => count++;

            _puzzle.ShowHint(); // level 1, now exhausted (HintCount == 1)
            _puzzle.ShowHint(); // should be ignored

            Assert.AreEqual(1, _puzzle.HintLevel);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void HintCountZeroRaisesNoEvent()
        {
            SetInfo(_puzzle, MakeInfo());
            _puzzle.Activate();
            int count = 0;
            _puzzle.HintRequested += _ => count++;

            _puzzle.ShowHint();

            Assert.AreEqual(0, count);
            Assert.AreEqual(0, _puzzle.HintLevel);
        }

        [Test]
        public void InfoNullKeepsOldHintBehavior()
        {
            _puzzle.Activate();
            int count = 0;
            _puzzle.HintRequested += _ => count++;

            _puzzle.ShowHint();
            _puzzle.ShowHint();
            _puzzle.ShowHint();

            Assert.AreEqual(3, count);
        }

        [Test]
        public void HasHintsLeftTrueWithHintsAndLevelBelowCount()
        {
            SetInfo(_puzzle, MakeInfo("line1", "line2"));
            _puzzle.Activate();

            Assert.IsTrue(_puzzle.HasHintsLeft);

            _puzzle.ShowHint();
            Assert.IsTrue(_puzzle.HasHintsLeft); // level 1 of 2, one remains
        }

        [Test]
        public void HasHintsLeftFalseWhenExhausted()
        {
            SetInfo(_puzzle, MakeInfo("line1"));
            _puzzle.Activate();

            _puzzle.ShowHint(); // level 1 of 1, now exhausted

            Assert.IsFalse(_puzzle.HasHintsLeft);
        }

        [Test]
        public void HasHintsLeftFalseWhenHintCountIsZero()
        {
            SetInfo(_puzzle, MakeInfo());
            _puzzle.Activate();

            Assert.IsFalse(_puzzle.HasHintsLeft);
        }

        [Test]
        public void HasHintsLeftFalseWhenNotActive()
        {
            SetInfo(_puzzle, MakeInfo("line1", "line2"));
            // never activated

            Assert.IsFalse(_puzzle.HasHintsLeft);
        }
    }
}