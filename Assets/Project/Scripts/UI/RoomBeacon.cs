using UnityEngine;
using Orbital.Core;

namespace Orbital.UI
{
    /// <summary>Shows a glowing marker over a room's puzzle while it is the one the player should be doing.</summary>
    public class RoomBeacon : MonoBehaviour
    {
        [SerializeField] private PuzzleBase _puzzle;
        [SerializeField] private GameObject _visual;
        [SerializeField] private float _hideWithin = 2.0f;
        [SerializeField] private GameObject _floorRing;
        [SerializeField] private float _ringHideWithin = 0.8f;

        private MissionManager _mission;
        private Camera _camera;
        private bool _shouldShow;

        private void Start()
        {
            _mission = FindFirstObjectByType<MissionManager>();
            _mission.PuzzleStarted += OnPuzzleStarted;
            _mission.MissionCompleted += OnMissionCompleted;
            Refresh();
        }

        private void Update()
        {
            if (_camera == null) _camera = Camera.main;

            bool nearCamera = false;
            bool nearRing = false;
            if (_camera != null)
            {
                Vector3 toCamera = _camera.transform.position - transform.position;
                toCamera.y = 0f;
                nearCamera = toCamera.magnitude < _hideWithin;

                if (_floorRing != null)
                {
                    Vector3 toCameraRing = _camera.transform.position - _floorRing.transform.position;
                    toCameraRing.y = 0f;
                    nearRing = toCameraRing.magnitude < _ringHideWithin;
                }
            }

            _visual.SetActive(_shouldShow && !nearCamera);
            if (_floorRing != null) _floorRing.SetActive(_shouldShow && !nearRing);
        }

        private void OnPuzzleStarted(PuzzleBase puzzle) => Refresh();

        private void OnMissionCompleted() => Refresh();

        private void Refresh()
        {
            _shouldShow = _mission.Current == _puzzle && _puzzle.State == PuzzleState.Active;
        }

        private void OnDestroy()
        {
            if (_mission == null) return;
            _mission.PuzzleStarted -= OnPuzzleStarted;
            _mission.MissionCompleted -= OnMissionCompleted;
        }
    }
}
