using KidClock.Modules.Games.ViewModels.Logic;
using KidClock.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KidClock.Tests
{
    [TestClass]
    public class LogicGamesViewModelTests
    {
        [TestMethod]
        public void SequenceTrain_Challenge_ChooseIncrementsTotal()
        {
            StaTest.Run(() =>
            {
                var ds = new FakeDataService();
                var vm = new SequenceTrainViewModel(ds);

                vm.StartChallenge();
                Assert.IsTrue(vm.IsRunning);
                Assert.AreEqual(4, vm.Options.Count);
                Assert.AreEqual(4, vm.Sequence.Count);

                int before = vm.TotalCount;
                vm.ChooseOption(vm.Options[0]);
                Assert.AreEqual(before + 1, vm.TotalCount);
            });
        }

        [TestMethod]
        public void SequenceTrain_Pause_DisablesInput()
        {
            StaTest.Run(() =>
            {
                var ds = new FakeDataService();
                var vm = new SequenceTrainViewModel(ds);

                vm.StartChallenge();
                vm.Pause();
                Assert.IsTrue(vm.IsPaused);

                int before = vm.TotalCount;
                vm.ChooseOption(vm.Options[0]);
                Assert.AreEqual(before, vm.TotalCount);
            });
        }

        [TestMethod]
        public void CategoryMarket_Challenge_ChooseIncrementsTotal()
        {
            StaTest.Run(() =>
            {
                var ds = new FakeDataService();
                var vm = new CategoryMarketViewModel(ds);

                vm.StartChallenge();
                Assert.IsTrue(vm.IsRunning);
                Assert.IsNotNull(vm.CurrentItem);
                Assert.IsTrue(vm.Buckets.Count >= 2);

                int before = vm.TotalCount;
                vm.ChooseBucket(vm.Buckets[0]);
                Assert.AreEqual(before + 1, vm.TotalCount);
            });
        }

        [TestMethod]
        public void PatternMatrix_Challenge_ChooseIncrementsTotal()
        {
            StaTest.Run(() =>
            {
                var ds = new FakeDataService();
                var vm = new PatternMatrixViewModel(ds);

                vm.StartChallenge();
                Assert.IsTrue(vm.IsRunning);
                Assert.AreEqual(4, vm.Options.Count);
                Assert.IsTrue(vm.Cells.Count > 0);

                int before = vm.TotalCount;
                vm.ChooseOption(vm.Options[0]);
                Assert.AreEqual(before + 1, vm.TotalCount);
            });
        }

        [TestMethod]
        public void CommandMaze_Challenge_RunIncrementsTotalAndClearsProgram()
        {
            StaTest.Run(() =>
            {
                var ds = new FakeDataService();
                var vm = new CommandMazeViewModel(ds);

                vm.StartChallenge();
                Assert.IsTrue(vm.IsRunning);

                vm.AddForward();
                Assert.AreEqual(1, vm.Program.Count);

                int before = vm.TotalCount;
                vm.Run();
                Assert.AreEqual(before + 1, vm.TotalCount);
                Assert.AreEqual(0, vm.Program.Count);
            });
        }
    }
}

