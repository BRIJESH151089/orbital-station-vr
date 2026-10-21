using System;
using NUnit.Framework;
using Orbital.Core;

namespace Orbital.Core.Tests
{
    public class SortingLogicTests
    {
        private SortingLogic _logic;
        private int _changed;
        private int _completed;
        private int _mistake;

        [SetUp]
        public void SetUp()
        {
            _logic = new SortingLogic(new[] { 0, 0, 1 });
            _changed = 0;
            _completed = 0;
            _mistake = 0;
            _logic.Changed += () => _changed++;
            _logic.Completed += () => _completed++;
            _logic.Mistake += () => _mistake++;
        }

        [Test]
        public void NullArrayThrows()
        {
            Assert.Throws<ArgumentException>(() => new SortingLogic(null));
        }

        [Test]
        public void EmptyArrayThrows()
        {
            Assert.Throws<ArgumentException>(() => new SortingLogic(new int[0]));
        }

        [Test]
        public void CorrectPlacementIncreasesCorrectCount()
        {
            _logic.Place(0, 0);

            Assert.AreEqual(1, _logic.CorrectCount);
            Assert.AreEqual(0, _logic.Mistakes);
        }

        [Test]
        public void WrongPlacementRaisesMistakeButNotCorrectCount()
        {
            _logic.Place(0, 1);

            Assert.AreEqual(0, _logic.CorrectCount);
            Assert.AreEqual(1, _logic.Mistakes);
            Assert.AreEqual(1, _mistake);
        }

        [Test]
        public void MovingCorrectToWrongDecreasesCorrectCount()
        {
            _logic.Place(0, 0);
            _logic.Place(0, 1);

            Assert.AreEqual(0, _logic.CorrectCount);
            Assert.AreEqual(1, _logic.Mistakes);
        }

        [Test]
        public void MovingWrongToRightIncreasesCorrectCount()
        {
            _logic.Place(0, 1);
            _logic.Place(0, 0);

            Assert.AreEqual(1, _logic.CorrectCount);
            Assert.AreEqual(1, _logic.Mistakes);
        }

        [Test]
        public void PlacingSameBinTwiceChangesNothing()
        {
            _logic.Place(0, 0);
            _logic.Place(0, 0);

            Assert.AreEqual(1, _logic.CorrectCount);
            Assert.AreEqual(1, _changed);
        }

        [Test]
        public void CompletedFiresExactlyOnceOnLastCorrectPlacement()
        {
            _logic.Place(0, 0);
            _logic.Place(1, 0);
            _logic.Place(2, 1);

            Assert.IsTrue(_logic.IsComplete);
            Assert.AreEqual(1, _completed);
        }

        [Test]
        public void RemoveOfCorrectItemDecreasesCorrectCount()
        {
            _logic.Place(0, 0);
            _logic.Remove(0);

            Assert.AreEqual(0, _logic.CorrectCount);
        }

        [Test]
        public void RemoveOfItemInNoBinIsNoOp()
        {
            _logic.Remove(0);

            Assert.AreEqual(0, _logic.CorrectCount);
            Assert.AreEqual(0, _changed);
        }

        [Test]
        public void OutOfRangeItemIsIgnoredForPlaceAndRemove()
        {
            _logic.Place(-1, 0);
            _logic.Place(3, 0);
            _logic.Remove(-1);
            _logic.Remove(3);

            Assert.AreEqual(0, _logic.CorrectCount);
            Assert.AreEqual(0, _logic.Mistakes);
            Assert.AreEqual(0, _changed);
        }
    }
}
