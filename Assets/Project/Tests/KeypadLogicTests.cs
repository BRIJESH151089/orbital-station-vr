using System;
using NUnit.Framework;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class KeypadLogicTests
    {
        private const string Code = "4827";
        private KeypadLogic _keypad;
        private int _accepted;
        private int _rejected;

        [SetUp]
        public void SetUp()
        {
            _keypad = new KeypadLogic(Code);
            _accepted = 0;
            _rejected = 0;
            _keypad.Accepted += () => _accepted++;
            _keypad.Rejected += () => _rejected++;
        }

        private void Enter(string digits)
        {
            foreach (char c in digits)
                _keypad.Press(c - '0');
        }

        [Test]
        public void CorrectCodeRaisesAcceptedOnce()
        {
            Enter("4827");

            Assert.AreEqual(1, _accepted);
            Assert.AreEqual(0, _rejected);
        }

        [Test]
        public void WrongCodeRaisesRejectedAndClearsEntry()
        {
            Enter("1111");

            Assert.AreEqual(0, _accepted);
            Assert.AreEqual(1, _rejected);
            Assert.AreEqual("", _keypad.Entry);
        }

        [Test]
        public void EntryShowsDigitsTypedSoFar()
        {
            Enter("48");

            Assert.AreEqual("48", _keypad.Entry);
            Assert.AreEqual(0, _accepted);
            Assert.AreEqual(0, _rejected);
        }

        [Test]
        public void ExtraDigitsAfterFullCodeAreIgnored()
        {
            Enter("4827");
            Enter("9");

            Assert.AreEqual(1, _accepted);
            Assert.AreEqual("4827", _keypad.Entry);
        }

        [Test]
        public void DigitsOutsideZeroToNineAreIgnored()
        {
            _keypad.Press(-1);
            _keypad.Press(10);

            Assert.AreEqual("", _keypad.Entry);
        }

        [Test]
        public void ClearEmptiesTheEntry()
        {
            Enter("48");
            _keypad.Clear();

            Assert.AreEqual("", _keypad.Entry);
        }

        [Test]
        public void CanRetryAfterWrongCode()
        {
            Enter("1111");
            Enter("4827");

            Assert.AreEqual(1, _rejected);
            Assert.AreEqual(1, _accepted);
        }

        [Test]
        public void EmptyCodeThrows()
        {
            Assert.Throws<ArgumentException>(() => new KeypadLogic(""));
            Assert.Throws<ArgumentException>(() => new KeypadLogic(null));
        }

    }
}