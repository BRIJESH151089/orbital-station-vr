using UnityEngine;
using Orbital.Core;
using Orbital.Activities;

namespace Orbital.Audio
{
    /// <summary>Plays short sound effects in reaction to existing gameplay events. Adds no logic to any puzzle.</summary>
    public class AudioFeedback : MonoBehaviour
    {
        [Header("One-shot clips")]
        [SerializeField] private AudioClip _uiPoke;
        [SerializeField] private AudioClip _codeError;
        [SerializeField] private AudioClip _codeOk;
        [SerializeField] private AudioClip _socketSnap;
        [SerializeField] private AudioClip _podPlace;
        [SerializeField] private AudioClip _leverPull;
        [SerializeField] private AudioClip _hintUsed;
        [SerializeField] private AudioClip _missionComplete;
        [SerializeField] private AudioClip _nextStationPing;

        [Header("Ambient loop")]
        [SerializeField] private AudioClip _ambientHum;
        [SerializeField] private float _ambientVolume = 0.15f;

        [Header("Mixing")]
        [SerializeField] private float _masterVolume = 0.6f;
        [SerializeField] private int _spatialPoolSize = 4;
        [SerializeField] private float _spatialMinDistance = 1f;
        [SerializeField] private float _spatialMaxDistance = 8f;

        private AudioSource[] _spatialPool;
        private int _poolIndex;
        private AudioSource _source2D;
        private AudioSource _ambientSource;

        private MissionManager _mission;
        private BriefingPuzzle _briefing;
        private KeypadPuzzle _keypad;
        private PowerCellPuzzle _powerCell;
        private GreenhousePuzzle _greenhouse;
        private CommandDeckPuzzle _commandDeck;

        private int _lastSocketFilledCount;
        private int _lastSequenceProgress;

        private void Awake()
        {
            AudioListener.volume = _masterVolume;

            _spatialPool = new AudioSource[Mathf.Max(1, _spatialPoolSize)];
            for (int i = 0; i < _spatialPool.Length; i++)
            {
                var child = new GameObject("SFX_" + i);
                child.transform.SetParent(transform, false);
                var source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 1f;
                source.minDistance = _spatialMinDistance;
                source.maxDistance = _spatialMaxDistance;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                _spatialPool[i] = source;
            }

            var twoD = new GameObject("SFX_2D");
            twoD.transform.SetParent(transform, false);
            _source2D = twoD.AddComponent<AudioSource>();
            _source2D.playOnAwake = false;
            _source2D.spatialBlend = 0f;

            var ambient = new GameObject("AmbientHum");
            ambient.transform.SetParent(transform, false);
            _ambientSource = ambient.AddComponent<AudioSource>();
            _ambientSource.playOnAwake = false;
            _ambientSource.loop = true;
            _ambientSource.clip = _ambientHum;
            _ambientSource.volume = _ambientVolume;
            _ambientSource.spatialBlend = 1f;
            _ambientSource.minDistance = 2f;
            _ambientSource.maxDistance = 15f;
            _ambientSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }

        private void Start()
        {
            if (_ambientHum != null) _ambientSource.Play();

            _mission = FindFirstObjectByType<MissionManager>();
            _briefing = FindFirstObjectByType<BriefingPuzzle>();
            _keypad = FindFirstObjectByType<KeypadPuzzle>();
            _powerCell = FindFirstObjectByType<PowerCellPuzzle>();
            _greenhouse = FindFirstObjectByType<GreenhousePuzzle>();
            _commandDeck = FindFirstObjectByType<CommandDeckPuzzle>();

            if (_mission != null)
            {
                _mission.PuzzleStarted += OnPuzzleStarted;
                _mission.MissionCompleted += OnMissionCompleted;
                _mission.HintShown += OnHintShown;
            }

            if (_briefing != null) _briefing.Solved += OnBriefingSolved;

            if (_keypad != null)
            {
                _keypad.Solved += OnPuzzleSolved;
                _keypad.Logic.Rejected += OnCodeError;
            }

            if (_powerCell != null)
            {
                _powerCell.Solved += OnPuzzleSolved;
                _lastSocketFilledCount = _powerCell.Tracker.FilledCount;
                _powerCell.Tracker.Changed += OnSocketChanged;
            }

            if (_greenhouse != null)
            {
                _greenhouse.Solved += OnPuzzleSolved;
                _greenhouse.PodPlaced += OnPodPlaced;
            }

            if (_commandDeck != null)
            {
                _commandDeck.Solved += OnPuzzleSolved;
                _commandDeck.Lever.Pulled += OnLeverPulled;
                _commandDeck.SequenceRejected += OnCommandDeckWrongButton;
                _lastSequenceProgress = _commandDeck.Sequence.Progress;
                _commandDeck.Sequence.Changed += OnCommandDeckSequenceChanged;
            }
        }

        private void OnDisable()
        {
            if (_mission != null)
            {
                _mission.PuzzleStarted -= OnPuzzleStarted;
                _mission.MissionCompleted -= OnMissionCompleted;
                _mission.HintShown -= OnHintShown;
            }

            if (_briefing != null) _briefing.Solved -= OnBriefingSolved;

            if (_keypad != null)
            {
                _keypad.Solved -= OnPuzzleSolved;
                _keypad.Logic.Rejected -= OnCodeError;
            }

            if (_powerCell != null)
            {
                _powerCell.Solved -= OnPuzzleSolved;
                _powerCell.Tracker.Changed -= OnSocketChanged;
            }

            if (_greenhouse != null)
            {
                _greenhouse.Solved -= OnPuzzleSolved;
                _greenhouse.PodPlaced -= OnPodPlaced;
            }

            if (_commandDeck != null)
            {
                _commandDeck.Solved -= OnPuzzleSolved;
                _commandDeck.Lever.Pulled -= OnLeverPulled;
                _commandDeck.SequenceRejected -= OnCommandDeckWrongButton;
                _commandDeck.Sequence.Changed -= OnCommandDeckSequenceChanged;
            }
        }

        private void OnBriefingSolved(PuzzleBase puzzle) => PlaySpatial(_uiPoke, puzzle.transform.position);

        private void OnPuzzleSolved(PuzzleBase puzzle) => PlaySpatial(_codeOk, puzzle.transform.position);

        private void OnCodeError() => PlaySpatial(_codeError, _keypad.transform.position);

        private void OnSocketChanged()
        {
            int current = _powerCell.Tracker.FilledCount;
            if (current > _lastSocketFilledCount) PlaySpatial(_socketSnap, _powerCell.transform.position);
            _lastSocketFilledCount = current;
        }

        private void OnPodPlaced(int podIndex, int binIndex, bool correct)
        {
            var pod = _greenhouse.transform.Find("Pod_" + podIndex);
            PlaySpatial(_podPlace, pod != null ? pod.position : _greenhouse.transform.position);
        }

        private void OnCommandDeckWrongButton() => PlaySpatial(_codeError, _commandDeck.transform.position);

        private void OnCommandDeckSequenceChanged()
        {
            int current = _commandDeck.Sequence.Progress;
            if (current > _lastSequenceProgress) PlaySpatial(_uiPoke, _commandDeck.transform.position);
            _lastSequenceProgress = current;
        }

        private void OnLeverPulled()
        {
            var lever = _commandDeck.transform.Find("Lever");
            PlaySpatial(_leverPull, lever != null ? lever.position : _commandDeck.transform.position);
        }

        private void OnHintShown(PuzzleBase puzzle, int level) => PlaySpatial(_hintUsed, puzzle.transform.position);

        private void OnPuzzleStarted(PuzzleBase puzzle) => Play2D(_nextStationPing);

        private void OnMissionCompleted() => Play2D(_missionComplete);

        private void PlaySpatial(AudioClip clip, Vector3 position)
        {
            if (clip == null || _spatialPool.Length == 0) return;
            var source = _spatialPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _spatialPool.Length;
            source.transform.position = position;
            source.PlayOneShot(clip);
        }

        private void Play2D(AudioClip clip)
        {
            if (clip == null) return;
            _source2D.PlayOneShot(clip);
        }
    }
}
