using System;

namespace Orbital.Core
{
    /// <summary>Tracks score, hints and solved puzzles. Plain C#, no Unity dependencies.</summary>
    public class ScoreService
    {
        public const int PointsPerPuzzle = 100;
        public const int HintPenalty = 10;

        public int Score { get; private set; }
        public int HintsUsed { get; private set; }
        public int PuzzlesSolved { get; private set; }

        private int _hintsThisPuzzle;

        /// <summary>Call when the player asks for a hint on the current puzzle.</summary>
        public void RecordHint()
        {
            HintsUsed++;
            _hintsThisPuzzle++;
        }

        /// <summary>Call when the current puzzle is solved. Awards points minus hint penalties.</summary>
        public void RecordPuzzleSolved()
        {
            int points = Math.Max(0, PointsPerPuzzle - _hintsThisPuzzle * HintPenalty);
            Score += points;
            PuzzlesSolved++;
            _hintsThisPuzzle = 0;
        }
    }
}