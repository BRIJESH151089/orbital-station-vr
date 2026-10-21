using System;
using UnityEngine;
using Orbital.Core;

namespace Orbital.Activities
{
    /// <summary>Room 3: sort every pod into its matching bin.</summary>
    public class GreenhousePuzzle : PuzzleBase
    {
        [SerializeField] private int[] _correctBins = { 0, 0, 0, 1, 1, 1 };

        public SortingLogic Logic { get; private set; }

        public override string ProgressText =>
            Logic == null ? string.Empty : $"Pods sorted {Logic.CorrectCount}/{Logic.Total}";

        /// <summary>Raised when a pod is placed in a bin: (podIndex, binIndex, correct).</summary>
        public event Action<int, int, bool> PodPlaced;

        private void Awake()
        {
            Logic = new SortingLogic(_correctBins);
            Logic.Changed += RaiseProgressChanged;
            Logic.Completed += MarkSolved;
        }

        /// <summary>Hook to a bin trigger's "pod entered" event with the pod's and bin's index.</summary>
        public void PodEnteredBin(int podIndex, int binIndex)
        {
            Logic.Place(podIndex, binIndex);
            bool correct = podIndex >= 0 && podIndex < _correctBins.Length && _correctBins[podIndex] == binIndex;
            PodPlaced?.Invoke(podIndex, binIndex, correct);
        }

        /// <summary>Hook to a bin trigger's "pod left" event with the pod's index.</summary>
        public void PodLeftBin(int podIndex) => Logic.Remove(podIndex);

        protected override void OnActivated()
        {
            // The player may have sorted pods before this room became active.
            if (Logic.IsComplete) MarkSolved();
        }

        private void OnDestroy()
        {
            if (Logic == null) return;
            Logic.Changed -= RaiseProgressChanged;
            Logic.Completed -= MarkSolved;
        }
    }
}
