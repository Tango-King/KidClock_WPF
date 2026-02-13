using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace KidClock.Tests.TestHelpers
{
    public static class StaTest
    {
        public static void Run(Action action)
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
            {
                throw new AssertFailedException(exception.ToString());
            }
        }
    }
}

