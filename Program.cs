using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Operating_Timer.Components;

namespace Operating_Timer
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDPIAware();
            }
            catch
            {
                // Optional: log or ignore
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var splash = new SplashForm())
            {
                var result = splash.ShowDialog();

                if (result == DialogResult.OK)
                {
                    Application.Run(new OperatingTimer());
                }
                else
                {
                    new ErrorDialog("Splash screen gagal dimuat.", "Kesalahan", ErrorDialog.DialogType.Error).ShowDialog();
                }
            }
        }
    }
}
