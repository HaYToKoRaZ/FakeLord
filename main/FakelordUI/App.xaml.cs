using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using FakelordUI.Core;

namespace FakelordUI
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _instanceMutex;
        public const string MutexName = "FakeLord_SingleInstance_Mutex_HaYToKoRaZ";
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOW_FAKELORD");

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        protected override void OnStartup(StartupEventArgs e)
        {
            _instanceMutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                // Zaten çalışan bir kopya var, mevcut pencereyi öne getir ve anında kapan
                PostMessage(HWND_BROADCAST, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _instanceMutex?.ReleaseMutex();
                _instanceMutex?.Dispose();
            }
            catch { }

            // Program kapatıldığında arkada kalan tüm sahte oyunları anında temizle!
            GhostProcessManager.StopGame();
            base.OnExit(e);
        }
    }
}
