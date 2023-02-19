using System;
using System.Diagnostics;
using System.Threading;
using Datagrove.Testing.Selenium;

namespace Datagrove.Testing.Selenium
{
    public abstract class WaitConditionsBase
    {
        protected readonly int _waitMs;
        private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(25);

        protected WaitConditionsBase(int waitMs) : base()
        {
            _waitMs = waitMs;
        }
        protected bool WaitFor(Func<bool> test, string exceptionMessage = "Waiting for Text to change.")
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            if (test()) return true;

            while (stopwatch.ElapsedMilliseconds <= _waitMs)
            {
                if (test()) return true;
                Thread.Sleep(_interval);
            }

            throw new WebDriverTimeoutException(exceptionMessage);
        }
    }
}