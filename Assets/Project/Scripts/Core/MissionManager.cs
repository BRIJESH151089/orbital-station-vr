using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orbital.Core
{
    /// <summary>Runs the activities in order and feeds the ScoreService. No XR code.</summary>
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private List<PuzzleBase> _puzzles = new List<PuzzleBase>();

        public ScoreService Score { get; } = new ScoreService();
        public int CurrentIndex { get; private set; } = -1;
        public bool IsComplete { get; private set; }

        public event Action<PuzzleBase> PuzzleStarted;
        public event Action MissionCompleted;
        public event Action<PuzzleBase, int> HintShown;
        public event Action<PuzzleBase, int> PuzzleScored;

        public int Count => _puzzles.Count;

        /// <summary>The puzzle currently being played, or null before start and after the end.</summary>
        public PuzzleBase Current =>
            CurrentIndex >= 0 && CurrentIndex < _puzzles.Count ? _puzzles[CurrentIndex] : null; private void Start()
        {
            BeginMission();
        }

        /// <summary>Replaces the puzzle list. Used by tests and by code that builds the mission.</summary>
        public void Initialize(IEnumerable<PuzzleBase> puzzles)
        {
            Unsubscribe();
            _puzzles = new List<PuzzleBase>(puzzles);
        }

        /// <summary>Subscribes to every puzzle and activates the first one.</summary>
        public void BeginMission()
        {
            Unsubscribe();
            foreach (var p in _puzzles)
            {
                p.Solved += OnPuzzleSolved;
                p.HintRequested += OnHintRequested;
            }

            CurrentIndex = -1;
            IsComplete = false;
            Advance();
        }

        private void OnPuzzleSolved(PuzzleBase puzzle)
        {
            if (puzzle.AwardsPoints)
            {
                int before = Score.Score;
                Score.RecordPuzzleSolved();
                PuzzleScored?.Invoke(puzzle, Score.Score - before);
            }
            Advance();
        }

        /// <summary>Requests a hint for the current puzzle. Does nothing if no puzzle is active.</summary>
        public void RequestHint() => Current?.ShowHint();

        private void OnHintRequested(PuzzleBase puzzle)
        {
            if (puzzle.AwardsPoints) Score.RecordHint();
            HintShown?.Invoke(puzzle, puzzle.HintLevel);
        }

        private void Advance()
        {
            CurrentIndex++;
            if (CurrentIndex >= _puzzles.Count)
            {
                IsComplete = true;
                MissionCompleted?.Invoke();
                return;
            }

            var next = _puzzles[CurrentIndex];
            next.Activate();
            PuzzleStarted?.Invoke(next);
        }

        private void Unsubscribe()
        {
            foreach (var p in _puzzles)
            {
                if (p == null) continue;
                p.Solved -= OnPuzzleSolved;
                p.HintRequested -= OnHintRequested;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}