using Orbital.Core;

namespace Orbital.Activities
{
    /// <summary>Mission briefing activity. Call Complete() from the start button; XR input lives in the Interaction layer.</summary>
    public class BriefingPuzzle : PuzzleBase
    {
        public override bool AwardsPoints => false;

        /// <summary>Hook this to the start button's event. Ignored unless the puzzle is Active.</summary>
        public void Complete()
        {
            if (State != PuzzleState.Active) return;
            MarkSolved();
        }
    }
}
