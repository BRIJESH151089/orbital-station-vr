using UnityEngine;

namespace Orbital.Activities
{
    /// <summary>Reports pods entering/leaving a bin's trigger volume. The pod needs a Rigidbody for trigger events to fire.</summary>
    public class BinTrigger : MonoBehaviour
    {
        [SerializeField] private GreenhousePuzzle _puzzle;
        [SerializeField] private int _binIndex;

        private void OnTriggerEnter(Collider other)
        {
            var pod = other.GetComponentInParent<PodId>();
            if (pod == null) return;
            _puzzle.PodEnteredBin(pod.Index, _binIndex);
        }

        private void OnTriggerExit(Collider other)
        {
            var pod = other.GetComponentInParent<PodId>();
            if (pod == null) return;
            _puzzle.PodLeftBin(pod.Index);
        }
    }
}
