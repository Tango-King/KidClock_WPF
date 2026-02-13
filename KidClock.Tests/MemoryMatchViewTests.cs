using KidClock.Modules.Games.ViewModels;
using KidClock.Modules.Games.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Windows.Controls;

namespace KidClock.Tests
{
    [TestClass]
    public class MemoryMatchViewTests
    {
        [TestMethod]
        public void DifficultyButtons_ShouldRebuildBoard()
        {
            RunInSta(() =>
            {
                var view = new MemoryMatchView();
                var vm = new MemoryMatchViewModel();

                view.DataContext = vm;

                var grid = (Grid)view.FindName("GameGridContainer")!;
                Assert.AreEqual(16, grid.Children.Count);

                vm.StartGameCommand.Execute("6");
                Assert.AreEqual(36, grid.Children.Count);

                vm.StartGameCommand.Execute("8");
                Assert.AreEqual(64, grid.Children.Count);
            });
        }

        private static void RunInSta(Action action)
        {
            Exception? exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
                throw new AssertFailedException(exception.ToString());
        }
    }
}

