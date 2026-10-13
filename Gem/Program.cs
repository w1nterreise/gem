using System;
using System.Threading;
using System.Windows.Forms;

namespace Gem {
    internal static class Program {

        private static Mutex mutex = new Mutex(true, "{B4C83F67-A912-421E-A358-8C376288DE91}");

        [STAThread]
        static void Main() {

            if (mutex.WaitOne(TimeSpan.Zero, true)) {
                try {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Gem());
                }
                finally {
                    mutex.ReleaseMutex();
                }
            }
            else {
                return;
            }
        }
    }
}
