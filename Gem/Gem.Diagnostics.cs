using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gem {
    public partial class Gem : Form {

        private async void MemoryTimer_Tick(object sender, EventArgs e) {

            _memoryTimer.Stop();

            long mainProcessBytes    = 0;
            long childProcessesBytes = 0;

            string mainProcessName = Process.GetCurrentProcess().ProcessName;

            await Task.Run(() => {
                try {
                    using (PerformanceCounter pc = new PerformanceCounter("Process", "Working Set - Private", mainProcessName)) {
                        mainProcessBytes = pc.RawValue;
                    }
                }
                catch {
                    using (Process currentProcess = Process.GetCurrentProcess()) {
                        currentProcess.Refresh();
                        mainProcessBytes = currentProcess.WorkingSet64;
                    }
                }

                try {
                    string query = "SELECT PrivatePageCount FROM Win32_Process WHERE Name = 'msedgewebview2.exe'";

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                    using (ManagementObjectCollection results = searcher.Get()) {
                        foreach (ManagementObject obj in results) {
                            try {
                                long privateBytes = Convert.ToInt64(obj["PrivatePageCount"]);
                                childProcessesBytes += privateBytes;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            });

            double mainMb    = (double)mainProcessBytes / (1024 * 1024);
            double childMb   = (double)childProcessesBytes / (1024 * 1024);
            tsLblMemory.Text = $"Memory: {mainMb:F1} Mb + ~{childMb:F1} Mb";

            _memoryTimer.Start();
        }


        private async void CacheTimer_Tick(object sender, EventArgs e) {
            _cacheTimer.Stop();
            await RunCacheCalculationAsync();
            _cacheTimer.Start();
        }


        private async Task RunCacheCalculationAsync() {

            long cacheBytes = 0;
            string path = _browserCachePath;

            await Task.Run(() => {
                try {
                    if (Directory.Exists(path)) {
                        DirectoryInfo di = new DirectoryInfo(path);
                        foreach (FileInfo fi in di.EnumerateFiles("*", SearchOption.AllDirectories)) {
                            try {
                                cacheBytes += fi.Length;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            });

            double cacheMb = (double)cacheBytes / (1024 * 1024);
            tsLblCache.Text = $"Cache: {cacheMb:F1} Mb";
        }
    }
}
