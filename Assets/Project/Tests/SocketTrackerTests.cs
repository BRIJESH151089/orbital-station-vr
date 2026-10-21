using System;
using NUnit.Framework;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class SocketTrackerTests
    {
        private SocketTracker _tracker;
        private int _changed;
        private int _completed;

        [SetUp]
        public void SetUp()
        {
            _tracker = new SocketTracker(3);
            _changed = 0;
            _completed = 0;
            _tracker.Changed += () => _changed++;
            _tracker.Completed += () => _completed++;
        }

        [Test]
        public void StartsEmpty()
        {
            Assert.AreEqual(0, _tracker.FilledCount);
            Assert.AreEqual(3, _tracker.Total);
            Assert.IsFalse(_tracker.IsComplete);
        }

        [Test]
        public void FillingAllSocketsCompletesOnce()
        {
            _tracker.Fill(0);
            _tracker.Fill(1);
            _tracker.Fill(2);

            Assert.IsTrue(_tracker.IsComplete);
            Assert.AreEqual(1, _completed);
            Assert.AreEqual(3, _changed);
        }

        [Test]
        public void FillingTheSameSocketTwiceCountsOnce()
        {
            _tracker.Fill(1);
            _tracker.Fill(1);

            Assert.AreEqual(1, _tracker.FilledCount);
            Assert.AreEqual(1, _changed);
        }

        [Test]
        public void EmptyingReducesCount()
        {
            _tracker.Fill(0);
            _tracker.Fill(1);
            _tracker.Empty(0);

            Assert.AreEqual(1, _tracker.FilledCount);
        }

        [Test]
        public void EmptyingAnEmptySocketDoesNothing()
        {
            _tracker.Empty(0);

            Assert.AreEqual(0, _tracker.FilledCount);
            Assert.AreEqual(0, _changed);
        }

        [Test]
        public void OutOfRangeIndexesAreIgnored()
        {
            _tracker.Fill(-1);
            _tracker.Fill(3);

            Assert.AreEqual(0, _tracker.FilledCount);
            Assert.AreEqual(0, _changed);
        }

        [Test]
        public void ReachingFullAgainRaisesCompletedAgain()
        {
            _tracker.Fill(0); _tracker.Fill(1); _tracker.Fill(2);
            _tracker.Empty(2);
            _tracker.Fill(2);

            Assert.AreEqual(2, _completed);
        }

        [Test]
        public void ZeroSocketsThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SocketTracker(0));
        }

        [Test]
        public void IsFilledTracksEachSocketIndependently()
        {
            _tracker.Fill(1);

            Assert.IsFalse(_tracker.IsFilled(0));
            Assert.IsTrue(_tracker.IsFilled(1));
            Assert.IsFalse(_tracker.IsFilled(2));
        }

        [Test]
        public void IsFilledOutOfRangeIndexIsSafeAndFalse()
        {
            Assert.IsFalse(_tracker.IsFilled(-1));
            Assert.IsFalse(_tracker.IsFilled(3));
        }
    }
}