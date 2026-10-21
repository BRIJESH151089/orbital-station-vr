using TMPro;
using UnityEngine;
using Orbital.Activities;
using Orbital.Core;

namespace Orbital.UI
{
    /// <summary>Shows the keypad entry on a world-space TextMeshPro label.</summary>
    public class KeypadDisplay : MonoBehaviour
    {
        [SerializeField] private KeypadPuzzle _puzzle;
        [SerializeField] private TMP_Text _text;

        private void Start()
        {
            // KeypadPuzzle creates Logic in Awake(), so it is ready by Start().
            _puzzle.Logic.EntryChanged += OnEntryChanged;
            _puzzle.Logic.Rejected += OnRejected;
            _puzzle.Solved += OnSolved;
            OnEntryChanged("");
        }

        private void OnEntryChanged(string entry)
        {
            _text.text = entry.Length == 0 ? "----" : entry;
        }

        private void OnRejected()
        {
            _text.text = "DENIED";
        }

        private void OnSolved(PuzzleBase puzzle)
        {
            _text.text = "OPEN";
        }

        private void OnDestroy()
        {
            if (_puzzle == null || _puzzle.Logic == null) return;
            _puzzle.Logic.EntryChanged -= OnEntryChanged;
            _puzzle.Logic.Rejected -= OnRejected;
            _puzzle.Solved -= OnSolved;
        }
    }
}