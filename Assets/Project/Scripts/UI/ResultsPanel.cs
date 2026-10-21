using TMPro;
using UnityEngine;
using Orbital.Core;

namespace Orbital.UI
{
    /// <summary>Shows the final score, hints and time once the mission completes.</summary>
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] private MissionManager _mission;
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _hintsText;
        [SerializeField] private TMP_Text _timeText;

        private float _startTime;

        private void Start()
        {
            _root.SetActive(false);
            _startTime = Time.time;
            _mission.MissionCompleted += OnCompleted;
        }

        private void OnCompleted()
        {
            _root.SetActive(true);

            int elapsed = Mathf.FloorToInt(Time.time - _startTime);
            int minutes = elapsed / 60;
            int seconds = elapsed % 60;

            _scoreText.text = $"Score {_mission.Score.Score}";
            _hintsText.text = $"Hints used {_mission.Score.HintsUsed}";
            _timeText.text = $"Time {minutes:00}:{seconds:00}";
        }

        private void OnDestroy()
        {
            if (_mission == null) return;
            _mission.MissionCompleted -= OnCompleted;
        }
    }
}
