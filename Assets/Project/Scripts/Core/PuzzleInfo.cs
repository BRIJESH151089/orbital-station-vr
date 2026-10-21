using UnityEngine;

namespace Orbital.Core
{
    /// <summary>Designer-editable text for one puzzle: title and instruction.</summary>
    [CreateAssetMenu(menuName = "Orbital/Puzzle Info", fileName = "PuzzleInfo")]
    public class PuzzleInfo : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _instruction;
        [SerializeField] private string _location;
        [SerializeField] private string[] _hints = System.Array.Empty<string>();

        public string Title => _title;
        public string Instruction => _instruction;
        public string Location => _location;
        public int HintCount => _hints.Length;

        /// <summary>The hint line for a 0-based level, or empty if out of range.</summary>
        public string GetHint(int level) => level >= 0 && level < _hints.Length ? _hints[level] : string.Empty;
    }
}