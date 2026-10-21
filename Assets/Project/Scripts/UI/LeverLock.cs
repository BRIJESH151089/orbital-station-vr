using UnityEngine;
using Orbital.Core;
using Orbital.Activities;

namespace Orbital.UI
{
    /// <summary>Disables the Command deck lever's grab interactables until the button sequence is complete.</summary>
    public class LeverLock : MonoBehaviour
    {
        [SerializeField] private CommandDeckPuzzle _puzzle;
        [SerializeField] private Behaviour[] _interactables;
        [SerializeField] private Transform _handle;

        private Quaternion _restRotation;
        private Rigidbody _handleBody;

        private void Awake()
        {
            if (_handle == null) return;
            _restRotation = _handle.localRotation;
            _handleBody = _handle.GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _puzzle.ProgressChanged += OnProgressChanged;
            _puzzle.Solved += OnSolved;
            Refresh();
        }

        private void OnProgressChanged(PuzzleBase puzzle) => Refresh();

        private void OnSolved(PuzzleBase puzzle) => Refresh();

        private void Refresh()
        {
            bool unlocked = _puzzle.SequenceComplete;

            foreach (var interactable in _interactables)
            {
                if (interactable != null) interactable.enabled = unlocked;
            }

            if (unlocked || _handle == null) return;

            _handle.localRotation = _restRotation;
            if (_handleBody == null || _handleBody.isKinematic) return;

            _handleBody.linearVelocity = Vector3.zero;
            _handleBody.angularVelocity = Vector3.zero;
        }

        private void OnDestroy()
        {
            if (_puzzle == null) return;
            _puzzle.ProgressChanged -= OnProgressChanged;
            _puzzle.Solved -= OnSolved;
        }
    }
}
