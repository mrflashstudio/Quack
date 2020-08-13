using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Quack.Helpers
{
    public class TimingHelper
    {
        delegate void TimerCallback(uint uTimerID, uint uMsg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeSetEvent(uint uDelay, uint uResolution, TimerCallback lpTimeProc, UIntPtr dwUser, uint fuEvent);

        [DllImport("Winmm.dll", CharSet = CharSet.Auto)]
        static extern uint timeKillEvent(uint uTimerID);

        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        public static void Delay(uint milliseconds)
        {
            uint timer = timeSetEvent(milliseconds, 0, callbackFunction, UIntPtr.Zero, 0);

            autoResetEvent.WaitOne();
            timeKillEvent(timer);
        }

        private static void callbackFunction(uint uTimerID, uint msg, UIntPtr dwUser, UIntPtr dw1, UIntPtr dw2) => autoResetEvent.Set();
    }
}
