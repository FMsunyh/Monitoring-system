using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace Log
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (ProcInstance() == true)
            {
                Application.Exit();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static bool ProcInstance()
        {
            int pid;
            if ((pid = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length) > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}