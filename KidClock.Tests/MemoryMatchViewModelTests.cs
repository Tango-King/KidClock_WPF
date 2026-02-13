using KidClock.Modules.Games.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace KidClock.Tests
{
    [TestClass]
    public class MemoryMatchViewModelTests
    {
        [TestMethod]
        public void StartGame_4x4_CreatesCorrectCardCount()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("4");
            Assert.AreEqual(4, vm.GridRows);
            Assert.AreEqual(4, vm.GridColumns);
            Assert.AreEqual(16, vm.Cards.Count);
        }

        [TestMethod]
        public void StartGame_6x6_CreatesCorrectCardCount()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("6");
            Assert.AreEqual(6, vm.GridRows);
            Assert.AreEqual(6, vm.GridColumns);
            Assert.AreEqual(36, vm.Cards.Count);
        }

        [TestMethod]
        public void StartGame_8x8_CreatesCorrectCardCount()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("8");
            Assert.AreEqual(8, vm.GridRows);
            Assert.AreEqual(8, vm.GridColumns);
            Assert.AreEqual(64, vm.Cards.Count);
        }

        [TestMethod]
        public void StartGame_Invalid_DefaultsTo4x4()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("999");
            Assert.AreEqual(4, vm.GridRows);
            Assert.AreEqual(4, vm.GridColumns);
            Assert.AreEqual(16, vm.Cards.Count);
        }

        [TestMethod]
        public async Task CardClick_Match_MarksMatchedAndAddsScore()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("4");

            var pair = vm.Cards
                .GroupBy(c => c.MatchId)
                .First(g => g.Count() == 2)
                .ToArray();

            await vm.CardClick(pair[0]);
            await vm.CardClick(pair[1]);

            Assert.IsTrue(pair[0].IsMatched);
            Assert.IsTrue(pair[1].IsMatched);
            Assert.AreEqual(10, vm.Score);
        }

        [TestMethod]
        public async Task CardClick_NotMatch_FlipsBackAfterDelay()
        {
            var vm = new MemoryMatchViewModel();
            vm.StartGame("4");

            var first = vm.Cards[0];
            var second = vm.Cards.First(c => c.MatchId != first.MatchId);

            await vm.CardClick(first);
            await vm.CardClick(second);

            Assert.IsFalse(first.IsFlipped);
            Assert.IsFalse(second.IsFlipped);
            Assert.AreEqual(0, vm.Score);
        }
    }
}

