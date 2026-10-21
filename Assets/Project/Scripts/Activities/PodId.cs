using UnityEngine;

namespace Orbital.Activities
{
    /// <summary>Identifies a greenhouse pod by index for BinTrigger to report.</summary>
    public class PodId : MonoBehaviour
    {
        [SerializeField] private int _index;

        public int Index => _index;
    }
}
