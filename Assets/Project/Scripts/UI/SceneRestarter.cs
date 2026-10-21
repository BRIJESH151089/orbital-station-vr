using UnityEngine;
using UnityEngine.SceneManagement;

namespace Orbital.UI
{
    /// <summary>Reloads the active scene, restarting the mission from the top.</summary>
    public class SceneRestarter : MonoBehaviour
    {
        public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
