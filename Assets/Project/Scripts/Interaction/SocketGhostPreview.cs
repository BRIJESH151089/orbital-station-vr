using UnityEngine;

namespace Orbital.Interaction
{
    /// <summary>Shows or hides a socket's ghost preview. Hook to a SnapInteractable's hover/select Unity events.</summary>
    public class SocketGhostPreview : MonoBehaviour
    {
        [SerializeField] private GameObject _ghost;

        public void Show()
        {
            if (_ghost != null) _ghost.SetActive(true);
        }

        public void Hide()
        {
            if (_ghost != null) _ghost.SetActive(false);
        }
    }
}
