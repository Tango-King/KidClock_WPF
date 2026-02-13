using KidClock.Core.Models;
using KidClock.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace KidClock.Tests
{
    [TestClass]
    public class SqliteDataServiceTests
    {
        [TestMethod]
        public void SaveSession_And_QueryBest_And_WeeklyReport_Works()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "KidClockTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            string old = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = tempDir;

                var ds = new SqliteDataService();
                ds.Initialize();

                var now = DateTime.UtcNow.ToString("O");
                ds.SaveGameSession(new GameSessionResult(
                    "TestGame",
                    "Challenge",
                    2,
                    8,
                    10,
                    90,
                    2,
                    now,
                    "tag1,tag2"
                ));

                var best = ds.GetBestRecord("TestGame", "Challenge");
                Assert.IsNotNull(best);
                Assert.AreEqual(2, best.StarRating);
                Assert.AreEqual(10, best.TotalCount);

                var report = ds.GetWeeklyReport(DateTime.UtcNow.AddDays(-7).ToString("O"), DateTime.UtcNow.AddDays(1).ToString("O"));
                Assert.IsTrue(report.Items.Length >= 1);
            }
            finally
            {
                Environment.CurrentDirectory = old;
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }
    }
}

