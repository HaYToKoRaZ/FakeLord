using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HaYTooL
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
        private readonly string _gameTitle;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public MainWindow(string gameTitle, string exeName, string imageUrl)
        {
            InitializeComponent();

            _gameTitle = string.IsNullOrWhiteSpace(gameTitle) ? "HaYTooL Ghost Runner" : gameTitle;
            this.Title = _gameTitle; // Discord process scanner checks this window title!

            TxtGameTitle.Text = _gameTitle;
            TxtExeName.Text = string.IsNullOrWhiteSpace(exeName) ? "HaYTooL.exe" : exeName;

            // Load Game Cover
            LoadGameImage(imageUrl);

            // Timer for Active Duration
            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    // Ensure Win32 window text matches the game title for Discord game detection
                    SetWindowText(hwnd, _gameTitle);

                    // Enable Windows Immersive Dark Mode
                    int darkMode = 1;
                    DwmSetWindowAttribute(hwnd, 20 /* DWMWA_USE_IMMERSIVE_DARK_MODE */, ref darkMode, sizeof(int));

                    // Enable Windows 11 Rounded Corners
                    int cornerPref = 2; /* DWMWCP_ROUND */
                    DwmSetWindowAttribute(hwnd, 33 /* DWMWA_WINDOW_CORNER_PREFERENCE */, ref cornerPref, sizeof(int));
                }
            }
            catch { }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TxtDuration.Text = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }

        private async void LoadGameImage(string imageUrl)
        {
            try
            {
                // 1. Pack URI veya Gömülü Kaynak Kontrolü (LoL / Valorant)
                if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var packBmp = new BitmapImage();
                        packBmp.BeginInit();
                        packBmp.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        packBmp.CacheOption = BitmapCacheOption.OnLoad;
                        packBmp.EndInit();
                        ImgGameCover.Source = packBmp;
                        TxtFallbackIcon.Visibility = Visibility.Collapsed;
                        return;
                    }
                    catch { }
                }

                // 2. Oyun başlığı veya argümana göre LoL / Valorant doğrudan tespiti
                if (_gameTitle.Contains("League of Legends", StringComparison.OrdinalIgnoreCase) || 
                    (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("lol", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var lolPackBmp = new BitmapImage();
                        lolPackBmp.BeginInit();
                        lolPackBmp.UriSource = new Uri("pack://application:,,,/lol.png", UriKind.Absolute);
                        lolPackBmp.CacheOption = BitmapCacheOption.OnLoad;
                        lolPackBmp.EndInit();
                        ImgGameCover.Source = lolPackBmp;
                        TxtFallbackIcon.Visibility = Visibility.Collapsed;
                        return;
                    }
                    catch { }
                }
                else if (_gameTitle.Contains("VALORANT", StringComparison.OrdinalIgnoreCase) || 
                         (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("valorant", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var valoPackBmp = new BitmapImage();
                        valoPackBmp.BeginInit();
                        valoPackBmp.UriSource = new Uri("pack://application:,,,/valorant.png", UriKind.Absolute);
                        valoPackBmp.CacheOption = BitmapCacheOption.OnLoad;
                        valoPackBmp.EndInit();
                        ImgGameCover.Source = valoPackBmp;
                        TxtFallbackIcon.Visibility = Visibility.Collapsed;
                        return;
                    }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                    return;

                // 3. Yerel dosya ve aday yollar kontrolü
                string fileName = Path.GetFileName(imageUrl);
                string[] candidatePaths = new[]
                {
                    imageUrl,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageUrl),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Cache", "Icons", fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Data", fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Data", "Cache", "Icons", fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Data", fileName)
                };

                foreach (var p in candidatePaths)
                {
                    if (!string.IsNullOrWhiteSpace(p) && File.Exists(p))
                    {
                        var localBitmap = new BitmapImage();
                        localBitmap.BeginInit();
                        localBitmap.UriSource = new Uri(p, UriKind.Absolute);
                        localBitmap.CacheOption = BitmapCacheOption.OnLoad;
                        localBitmap.EndInit();
                        ImgGameCover.Source = localBitmap;
                        TxtFallbackIcon.Visibility = Visibility.Collapsed;
                        return;
                    }
                }

                // 3. Discord CDN URL ise User-Agent ile indir (WPF 403 Forbidden almaması için)
                if (imageUrl.Contains("cdn.discordapp.com/app-icons/"))
                {
                    string? downloadedLocalPath = null;
                    await Task.Run(async () =>
                    {
                        try
                        {
                            using var client = new System.Net.Http.HttpClient();
                            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                            byte[] bytes = await client.GetByteArrayAsync(imageUrl);
                            string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", "Icons");
                            Directory.CreateDirectory(cacheDir);
                            string safeName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                            string targetPath = Path.Combine(cacheDir, safeName);
                            await File.WriteAllBytesAsync(targetPath, bytes);
                            downloadedLocalPath = targetPath;
                        }
                        catch { }
                    });

                    if (!string.IsNullOrEmpty(downloadedLocalPath) && File.Exists(downloadedLocalPath))
                    {
                        var downloadedBmp = new BitmapImage();
                        downloadedBmp.BeginInit();
                        downloadedBmp.UriSource = new Uri(downloadedLocalPath, UriKind.Absolute);
                        downloadedBmp.CacheOption = BitmapCacheOption.OnLoad;
                        downloadedBmp.EndInit();
                        ImgGameCover.Source = downloadedBmp;
                        TxtFallbackIcon.Visibility = Visibility.Collapsed;
                        return;
                    }
                }

                // 4. Standart Web URL (Steam vs.)
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                bitmap.DownloadCompleted += (s, e) =>
                {
                    ImgGameCover.Source = bitmap;
                    TxtFallbackIcon.Visibility = Visibility.Collapsed;
                };

                bitmap.DownloadFailed += (s, e) =>
                {
                    TxtFallbackIcon.Visibility = Visibility.Visible;
                };

                ImgGameCover.Source = bitmap;
                TxtFallbackIcon.Visibility = Visibility.Collapsed;
            }
            catch
            {
                TxtFallbackIcon.Visibility = Visibility.Visible;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ShutdownRunner();
        }

        private void BtnStopGame_Click(object sender, RoutedEventArgs e)
        {
            ShutdownRunner();
        }

        private void ShutdownRunner()
        {
            _timer.Stop();
            _stopwatch.Stop();
            Application.Current.Shutdown();
        }
    }
}
