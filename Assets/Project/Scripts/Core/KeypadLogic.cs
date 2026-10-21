using System;
using System.Text;

namespace Orbital.Core
{
    /// <summary>Digit-entry logic for a keypad. Auto-checks when the code length is reached.</summary>
    public class KeypadLogic
    {
        private readonly string _code;
        private readonly StringBuilder _entry = new StringBuilder();

        public string Entry => _entry.ToString();

        public event Action<string> EntryChanged;
        public event Action Accepted;
        public event Action Rejected;

        public KeypadLogic(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Code required.", nameof(code));
            _code = code;
        }

        public void Press(int digit)
        {
            if (digit < 0 || digit > 9) return;
            if (_entry.Length >= _code.Length) return;

            _entry.Append(digit);
            EntryChanged?.Invoke(Entry);

            if (_entry.Length < _code.Length) return;

            if (Entry == _code)
            {
                Accepted?.Invoke();
            }
            else
            {
                Clear();
                Rejected?.Invoke();
            }
        }

        public void Clear()
        {
            _entry.Clear();
            EntryChanged?.Invoke(Entry);
        }
    }
}