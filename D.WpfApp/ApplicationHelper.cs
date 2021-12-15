using System;
using System.Threading;

namespace D.WpfApp {
    public static class ApplicationHelper {
        private static readonly string MyApplicationKey = "a7a0d62e-187f-46a0-8773-b32e6052cd8d";

        private static Mutex _instanceMutex;

        // https://xcalibursystems.com/restricting-wpf-applications-run-mutex/
        public static bool ReserveMutex() {
            // Set mutex
            _instanceMutex = new Mutex(true, MyApplicationKey);

            // Check if already running
            var isAlreadyInUse = false;
            try {
                isAlreadyInUse = !_instanceMutex.WaitOne(TimeSpan.Zero, true);
            } catch (AbandonedMutexException) {
                FreeMutex();
                isAlreadyInUse = false;
            } catch (Exception) {
                _instanceMutex.Close();
                isAlreadyInUse = false;
            }

            return isAlreadyInUse;
        }

        /// <summary>
        ///     Kills the instance.
        /// </summary>
        /// <param name="code">The code.</param>
        public static void FreeMutex(int code = 0) {
            if (_instanceMutex == null) return;

            try {
                _instanceMutex.ReleaseMutex();
            } catch (Exception) {
            }

            _instanceMutex.Close();
        }
    }
}