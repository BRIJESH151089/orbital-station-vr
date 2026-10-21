using NUnit.Framework;
using UnityEngine;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class MissionManagerTests
    {
        private class TestPuzzle : PuzzleBase
        {
            public void Solve() => MarkSolved();
        }

        private class NoPointsTestPuzzle : PuzzleBase
        {
            public override bool AwardsPoints => false;
            public void Solve() => MarkSolved();
        }

        private GameObject _go;
        private MissionManager _mission;
        private TestPuzzle _a, _b;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Mission");
            _mission = _go.AddComponent<MissionManager>();
            _a = _go.AddComponent<TestPuzzle>();
            _b = _go.AddComponent<TestPuzzle>();
            _mission.Initialize(new PuzzleBase[] { _a, _b });
            _mission.BeginMission();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

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
        public void FirstPuzzleActiveSecondLocked()
        {
            Assert.AreEqual(PuzzleState.Active, _a.State);
            Assert.AreEqual(PuzzleState.Locked, _b.State);
        }

        [Test]
        public void SolvingFirstActivatesSecond()
        {
            _a.Solve();
            Assert.AreEqual(PuzzleState.Active, _b.State);
            Assert.AreEqual(100, _mission.Score.Score);
        }

        [Test]
        public void SolvingAllCompletesMissionOnce()
        {
            int completed = 0;
            _mission.MissionCompleted += () => completed++;
            _a.Solve();
            _b.Solve();
            Assert.IsTrue(_mission.IsComplete);
            Assert.AreEqual(1, completed);
        }

        [Test]
        public void HintIsCountedAndPenalised()
        {
            _a.ShowHint();
            _a.Solve();
            Assert.AreEqual(1, _mission.Score.HintsUsed);
            Assert.AreEqual(90, _mission.Score.Score);
        }
        [Test]
        public void CurrentFollowsTheMission()
        {
            Assert.AreSame(_a, _mission.Current);
            _a.Solve();
            Assert.AreSame(_b, _mission.Current);
            _b.Solve();
            Assert.IsNull(_mission.Current);
            Assert.AreEqual(2, _mission.Count);
        }

        [Test]
        public void PuzzleWithDefaultAwardsPointsScoresFull()
        {
            Assert.IsTrue(_a.AwardsPoints);
            _a.Solve();
            Assert.AreEqual(100, _mission.Score.Score);
        }

        [Test]
        public void PuzzleWithoutPointsAdvancesButDoesNotScore()
        {
            var go = new GameObject("NoPointsMission");
            var mission = go.AddComponent<MissionManager>();
            var noPoints = go.AddComponent<NoPointsTestPuzzle>();
            var normal = go.AddComponent<TestPuzzle>();
            mission.Initialize(new PuzzleBase[] { noPoints, normal });
            mission.BeginMission();

            noPoints.Solve();
            int indexAfterFirst = mission.CurrentIndex;
            normal.Solve();

            Assert.AreEqual(1, indexAfterFirst);
            Assert.AreEqual(100, mission.Score.Score);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HintOnNoPointsPuzzleIsNotCountedOrPenalised()
        {
            var go = new GameObject("HintNoPointsMission");
            var mission = go.AddComponent<MissionManager>();
            var noPoints = go.AddComponent<NoPointsTestPuzzle>();
            var normal = go.AddComponent<TestPuzzle>();
            mission.Initialize(new PuzzleBase[] { noPoints, normal });
            mission.BeginMission();

            noPoints.ShowHint();
            Assert.AreEqual(0, mission.Score.HintsUsed);

            noPoints.Solve();
            normal.Solve();

            Assert.AreEqual(0, mission.Score.HintsUsed);
            Assert.AreEqual(100, mission.Score.Score);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RequestHintCallsCurrentPuzzleAndCostsTenOnlyForRealHints()
        {
            var go = new GameObject("HintCostMission");
            var mission = go.AddComponent<MissionManager>();
            var puzzle = go.AddComponent<TestPuzzle>();
            SetInfo(puzzle, MakeInfo("hint1", "hint2"));
            mission.Initialize(new PuzzleBase[] { puzzle });
            mission.BeginMission();

            mission.RequestHint();

            Assert.AreEqual(1, puzzle.HintLevel);
            Assert.AreEqual(1, mission.Score.HintsUsed);

            puzzle.Solve();
            Assert.AreEqual(90, mission.Score.Score);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void ExhaustedHintsCostNothing()
        {
            var go = new GameObject("ExhaustedHintMission");
            var mission = go.AddComponent<MissionManager>();
            var puzzle = go.AddComponent<TestPuzzle>();
            SetInfo(puzzle, MakeInfo("hint1"));
            mission.Initialize(new PuzzleBase[] { puzzle });
            mission.BeginMission();

            mission.RequestHint(); // real hint
            mission.RequestHint(); // exhausted, no-op
            mission.RequestHint(); // exhausted, no-op

            Assert.AreEqual(1, mission.Score.HintsUsed);
            Assert.AreEqual(1, puzzle.HintLevel);

            puzzle.Solve();
            Assert.AreEqual(90, mission.Score.Score); // only one hint's penalty applied

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HintShownCarriesTheLevel()
        {
            var go = new GameObject("HintShownMission");
            var mission = go.AddComponent<MissionManager>();
            var puzzle = go.AddComponent<TestPuzzle>();
            SetInfo(puzzle, MakeInfo("hint1", "hint2"));
            mission.Initialize(new PuzzleBase[] { puzzle });
            mission.BeginMission();

            PuzzleBase shownPuzzle = null;
            int shownLevel = -1;
            mission.HintShown += (p, level) => { shownPuzzle = p; shownLevel = level; };

            mission.RequestHint();
            Assert.AreSame(puzzle, shownPuzzle);
            Assert.AreEqual(1, shownLevel);

            mission.RequestHint();
            Assert.AreEqual(2, shownLevel);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PuzzleScoredRaisesCorrectPointsWithNoHints()
        {
            PuzzleBase scoredPuzzle = null;
            int scoredPoints = -1;
            _mission.PuzzleScored += (p, points) => { scoredPuzzle = p; scoredPoints = points; };

            _a.Solve();

            Assert.AreSame(_a, scoredPuzzle);
            Assert.AreEqual(100, scoredPoints);
        }

        [Test]
        public void PuzzleScoredRaisesCorrectPointsWithOneHint()
        {
            int scoredPoints = -1;
            _mission.PuzzleScored += (p, points) => scoredPoints = points;

            _a.ShowHint();
            _a.Solve();

            Assert.AreEqual(90, scoredPoints);
        }

        [Test]
        public void PuzzleScoredNotRaisedWhenPuzzleDoesNotAwardPoints()
        {
            var go = new GameObject("NoPointsScoredMission");
            var mission = go.AddComponent<MissionManager>();
            var noPoints = go.AddComponent<NoPointsTestPuzzle>();
            var normal = go.AddComponent<TestPuzzle>();
            mission.Initialize(new PuzzleBase[] { noPoints, normal });
            mission.BeginMission();

            bool raised = false;
            mission.PuzzleScored += (p, points) => raised = true;

            noPoints.Solve();

            Assert.IsFalse(raised);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void RequestHintWithNoCurrentPuzzleDoesNothing()
        {
            var go = new GameObject("NoCurrentMission");
            var mission = go.AddComponent<MissionManager>();
            var puzzle = go.AddComponent<TestPuzzle>();
            SetInfo(puzzle, MakeInfo("hint1"));
            mission.Initialize(new PuzzleBase[] { puzzle });
            mission.BeginMission();
            puzzle.Solve(); // only puzzle solved -> Current is null, mission complete

            bool shown = false;
            mission.HintShown += (p, level) => shown = true;

            mission.RequestHint();

            Assert.IsFalse(shown);

            Object.DestroyImmediate(go);
        }
    }
}