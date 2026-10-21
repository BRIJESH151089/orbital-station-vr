using TMPro;
using UnityEngine;
using Orbital.Core;

namespace Orbital.UI
{
    /// <summary>Shows the current room title, instruction, progress and score.</summary>
    public class MissionPanel : MonoBehaviour
    {
        [SerializeField] private MissionManager _mission;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _instruction;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private TMP_Text _score;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private TMP_Text _location;
        [SerializeField] private GameObject _hintButton;
        [SerializeField] private TMP_Text _hintButtonLabel;
        [SerializeField] private TMP_Text _feedback;
        [SerializeField] private PuzzleBase[] _localPuzzles = System.Array.Empty<PuzzleBase>();

        private const float FeedbackDuration = 5f;

        private PuzzleBase _current;
        private float _feedbackTimer = -1f;

        private void Start()
        {
            // Prefab instances placed in other rooms can't hold a reference to the
            // single scene MissionManager, so find it when the field wasn't wired.
            if (_mission == null) _mission = FindFirstObjectByType<MissionManager>();

            _mission.PuzzleStarted += OnPuzzleStarted;
            _mission.MissionCompleted += OnCompleted;
            _mission.HintShown += OnHintShown;
            _mission.PuzzleScored += OnPuzzleScored;

            // MissionManager may have started before this panel, so read the state now.
            if (_mission.IsComplete) OnCompleted();
            else if (_mission.Current != null) OnPuzzleStarted(_mission.Current);
        }

        private void Update()
        {
            if (_feedbackTimer < 0f) return;

            _feedbackTimer -= Time.deltaTime;
            if (_feedbackTimer <= 0f)
            {
                _feedbackTimer = -1f;
                if (_feedback != null) _feedback.text = string.Empty;
            }
        }

        /// <summary>Hook to the panel's own HINT button. Forwards to the shared MissionManager, which a prefab cannot reference directly.</summary>
        public void RequestHint() => _mission.RequestHint();

        private void OnPuzzleStarted(PuzzleBase puzzle)
        {
            Detach();
            _current = puzzle;
            _current.ProgressChanged += OnProgressChanged;
            _hint.text = string.Empty;
            Refresh();
            RefreshHintButton();
        }

        private void OnProgressChanged(PuzzleBase puzzle) => Refresh();

        private void OnHintShown(PuzzleBase puzzle, int level)
        {
            if (puzzle != _current || _current.Info == null) return;
            _hint.text = _current.Info.GetHint(level - 1);
            _score.text = $"Score {_mission.Score.Score}";
            RefreshHintButton();
        }

        private void OnCompleted()
        {
            Detach();
            _title.text = "Mission complete";
            _instruction.text = string.Empty;
            _progress.text = string.Empty;
            _score.text = string.Empty;
            _hint.text = string.Empty;
            if (_location != null) _location.text = string.Empty;
            if (_feedback != null) _feedback.text = string.Empty;
            _feedbackTimer = -1f;
            RefreshHintButton();
        }

        private void OnPuzzleScored(PuzzleBase puzzle, int points)
        {
            if (_feedback == null) return;

            string title = puzzle.Info != null ? puzzle.Info.Title : puzzle.name;
            _feedback.text = $"{title} complete  +{points}";
            _feedbackTimer = FeedbackDuration;
        }

        private void RefreshHintButton()
        {
            if (_hintButton == null) return;

            bool visible = _current != null && _current.HasHintsLeft;
            _hintButton.SetActive(visible);
            if (!visible || _hintButtonLabel == null) return;

            int remaining = _current.Info.HintCount - _current.HintLevel;
            _hintButtonLabel.text = remaining == 1 ? "LAST HINT -10" : "HINT -10";
        }

        private void Refresh()
        {
            var info = _current.Info;
            _title.text = info != null ? info.Title : _current.name;
            _instruction.text = info != null ? info.Instruction : string.Empty;
            if (_location != null)
            {
                bool inLocalRoom = System.Array.IndexOf(_localPuzzles, _current) >= 0;
                string location = info != null ? info.Location : string.Empty;
                _location.text = inLocalRoom || string.IsNullOrEmpty(location) ? string.Empty : location;
            }

            string step = $"Step {_mission.CurrentIndex + 1} of {_mission.Count}";
            _progress.text = string.IsNullOrEmpty(_current.ProgressText)
                ? step
                : $"{step}   {_current.ProgressText}";
            _score.text = $"Score {_mission.Score.Score}";
        }

        private void Detach()
        {
            if (_current != null) _current.ProgressChanged -= OnProgressChanged;
            _current = null;
        }

        private void OnDestroy()
        {
            Detach();
            if (_mission == null) return;
            _mission.PuzzleStarted -= OnPuzzleStarted;
            _mission.MissionCompleted -= OnCompleted;
            _mission.HintShown -= OnHintShown;
            _mission.PuzzleScored -= OnPuzzleScored;
        }
    }
}