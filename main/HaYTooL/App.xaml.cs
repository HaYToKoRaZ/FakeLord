using System;
using System.Windows;

namespace HaYTooL
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string title = "HaYTooL Ghost Runner";
            string exe = "HaYTooL.exe";
            string imageUrl = "";

            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i].Equals("--title", StringComparison.OrdinalIgnoreCase) && i + 1 < e.Args.Length)
                {
                    title = e.Args[++i];
                }
                else if (e.Args[i].Equals("--exe", StringComparison.OrdinalIgnoreCase) && i + 1 < e.Args.Length)
                {
                    exe = e.Args[++i];
                }
                else if (e.Args[i].Equals("--image", StringComparison.OrdinalIgnoreCase) && i + 1 < e.Args.Length)
                {
                    imageUrl = e.Args[++i];
                }
            }

            var mainWindow = new MainWindow(title, exe, imageUrl);
            mainWindow.Show();
        }
    }
}
