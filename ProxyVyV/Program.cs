namespace ProxyVyV
{
    using System;
    using System.Threading;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool flag;
            Mutex mutex = new Mutex(true, "VyVPROXY", out flag);
            if (!flag)
            {
                MessageBox.Show("Another instance is already running.");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ProxyVyV.Init());
                GC.KeepAlive(mutex);
            }
        }
    }
}

