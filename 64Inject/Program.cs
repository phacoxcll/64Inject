using System;
using System.Windows.Forms;

namespace _64Inject
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                FreeConsole();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new _64InjectGUI());
            }
            else
            {
                _64InjectCMD cmd = new _64InjectCMD();
                cmd.Run(args);
            }
        }
    }
}
