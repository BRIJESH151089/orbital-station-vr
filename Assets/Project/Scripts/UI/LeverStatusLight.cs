using System.Collections;
using UnityEngine;
using TMPro;
using Orbital.Core;
using Orbital.Activities;

namespace Orbital.UI
{
    /// <summary>Shows whether the Command deck lever is ready to pull.</summary>
    public class LeverStatusLight : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private CommandDeckPuzzle _puzzle;
        [SerializeField] private Renderer _lamp;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _lockedColor = Color.red;
        [SerializeField] private Color _readyColor = Color.green;
        [SerializeField] private string _lockedText = "LOCKED - enter the sequence";
        [SerializeField] private string _readyText = "READY - pull the lever";
        [SerializeField] private string _rejectedText = "WRONG - sequence reset";
        [SerializeField] private float _rejectedDuration = 1f;

        private MaterialPropertyBlock _block;
        private Coroutine _rejection;
        private bool _showingRejection;

        private void Start()
        {
            _block = new MaterialPropertyBlock();
            _puzzle.ProgressChanged += OnProgressChanged;
            _puzzle.Solved += OnSolved;
            _puzzle.SequenceRejected += OnSequenceRejected;
            Refresh();
        }

        private void OnProgressChanged(PuzzleBase puzzle) => Refresh();

        private void OnSolved(PuzzleBase puzzle) => Refresh();

        private void OnSequenceRejected()
        {
            if (_rejection != null) StopCoroutine(_rejection);
            _rejection = StartCoroutine(ShowRejection());
        }

        private IEnumerator ShowRejection()
        {
            _showingRejection = true;
            _label.text = _rejectedText;

            yield return new WaitForSeconds(_rejectedDuration);

            _showingRejection = false;
            _rejection = null;
            Refresh();
        }

        private void Refresh()
        {
            bool ready = _puzzle.SequenceComplete;
            Color color = ready ? _readyColor : _lockedColor;

            _lamp.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(EmissionColorId, color);
            _lamp.SetPropertyBlock(_block);

            if (!_showingRejection) _label.text = ready ? _readyText : _lockedText;
        }

        private void OnDestroy()
        {
            if (_puzzle == null) return;
            _puzzle.ProgressChanged -= OnProgressChanged;
            _puzzle.Solved -= OnSolved;
            _puzzle.SequenceRejected -= OnSequenceRejected;
        }
    }
}
