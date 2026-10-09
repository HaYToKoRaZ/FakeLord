using System;
using System.Linq;
using System.Windows;
using FakelordUI.Core;

namespace FakelordUI
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Program kapatıldığında arkada kalan tüm sahte oyunları anında temizle!
            GhostProcessManager.StopGame();
            base.OnExit(e);
        }
    }
}
