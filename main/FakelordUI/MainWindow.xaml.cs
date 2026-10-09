using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FakelordUI.Core;
using FakelordUI.Presets;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Cursors = System.Windows.Input.Cursors;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Windows.Controls.Image;
using Orientation = System.Windows.Controls.Orientation;
using Size = System.Windows.Size;

namespace FakelordUI
{
    public partial class MainWindow : Window
    {
        private List<GameItem> _allGames = new();
        private List<GameItem> _currentViewGames = new();
        private GameItem? _selectedGame;
        private bool _isPlaying = false;
        private string _activeTab = "All"; // All, SteamTop, SteamInstalled
        private bool _isUpdatingSearchText = false;
        private bool _isPopulatingThemes = false;
        private bool _isPopulatingSettingsThemes = false;
        private Forms.NotifyIcon? _notifyIcon;
        private bool _isExplicitExit = false;
        private bool _trayBalloonShownOnce = false;

        private readonly Dictionary<char, Button> _alphaButtons = new();
        private char? _currentActiveAlphaChar = null;

        private record ThemeInfo(
            string Key,
            string TrName,
            string EnName,
            string WinBg,
            string CardBg,
            string InnerBg,
            string BorderCol,
            string AccentCol,
            string AccentHover,
            string TextAccent);

        private static readonly List<ThemeInfo> AvailableThemes = new()
        {
            new("DiscordNitro", "🎮 Discord Nitro", "🎮 Discord Nitro", "#1E1F22", "#2B2D31", "#313338", "#3E4249", "#5865F2", "#4752C4", "#FFFFFF"),
            new("MidnightGalaxy", "🌌 Gece Galaksisi", "🌌 Midnight Galaxy", "#070612", "#0E0C22", "#151334", "#29245C", "#8B5CF6", "#7C3AED", "#FFFFFF"),
            new("TechInnovation", "⚡ Siber İnovasyon", "⚡ Tech Innovation", "#050811", "#0B1220", "#111C30", "#1D3252", "#00F0FF", "#00D4E2", "#050811"),
            new("OceanDepths", "🌊 Okyanus Derinlikleri", "🌊 Ocean Depths", "#040914", "#081426", "#0E203C", "#183764", "#38BDF8", "#0EA5E9", "#040914"),
            new("SunsetBoulevard", "🌆 Günbatımı Bulvarı", "🌆 Sunset Boulevard", "#0E0812", "#190F20", "#25172F", "#432654", "#F43F5E", "#E11D48", "#FFFFFF"),
            new("ForestCanopy", "🌲 Zümrüt Orman", "🌲 Forest Canopy", "#040D08", "#091A11", "#0F281B", "#19422D", "#10B981", "#059669", "#FFFFFF"),
            new("GoldenHour", "🌅 Altın Saat", "🌅 Golden Hour", "#0C0904", "#181208", "#241C0E", "#3F3018", "#F59E0B", "#D97706", "#0C0904"),
            new("ArcticFrost", "❄️ Kutup Ayazı", "❄️ Arctic Frost", "#05090F", "#0A131E", "#101D2C", "#1B3248", "#67E8F9", "#22D3EE", "#05090F"),
            new("DesertRose", "🏜️ Çöl Gülü", "🏜️ Desert Rose", "#0E080C", "#190F17", "#251723", "#44263E", "#FB7185", "#F43F5E", "#FFFFFF"),
            new("BotanicalGarden", "🌿 Botanik Bahçe", "🌿 Botanical Garden", "#050B08", "#0B1711", "#12241C", "#1E3E2F", "#34D399", "#10B981", "#050B08"),
            new("ModernMinimalist", "🖤 Modern Minimalist", "🖤 Modern Minimalist", "#08090C", "#111319", "#191C25", "#2B303E", "#E2E8F0", "#CBD5E1", "#08090C"),
            new("RogueCrimson", "🎯 Hayalet Kızıl", "🎯 Rogue Crimson", "#0C0608", "#160B0F", "#211117", "#3D1B27", "#FF2A54", "#E01E45", "#FFFFFF")
        };

        public MainWindow()
        {
            InitializeComponent();
            GhostProcessManager.InitializeRunnersDirectory();
            IniManager.Load();

            PopulateThemeComboBox();
            ComboTheme.SelectionChanged += ComboTheme_SelectionChanged;

            ApplyTheme(IniManager.Theme);
            ApplyLanguage(IniManager.Language);

            RestoreWindowGeometry();
            SetupSystemTray();

            // HaYTooL (Oyun) açıldığında FakelordUI yok olsun, kapandığında geri gelsin (Hand-off / Yer Değiştirme)
            GhostProcessManager.GameStarted += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    this.Hide();
                });
            };

            GhostProcessManager.GameExited += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    _isPlaying = false;
                    BtnStart.IsEnabled = true;
                    BtnStop.IsEnabled = false;

                    StatusMessage.Text = LocalizationManager.Get("StoppedStatusMsg");
                    StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
                    PreviewStatus.Text = LocalizationManager.Get("StoppedStatus");
                    UpdateTrayContextMenu();

                    BringWindowToFront();
                });
            };

            PreviewKeyDown += MainWindow_PreviewKeyDown;
            Closing += MainWindow_Closing;
            LocationChanged += MainWindow_LocationChanged;
            SizeChanged += MainWindow_SizeChanged;

            LoadInitialGames();
            RenderFavoritesBar();

            _ = CheckForGitHubUpdatesAsync();

            if (IniManager.AutoFetchDiscord)
            {
                _ = AutoUpdateDiscordCatalogAsync();
            }

            if (IniManager.StartMinimized)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    if (IniManager.EnableSystemTray && _notifyIcon != null)
                    {
                        Hide();
                    }
                    else
                    {
                        WindowState = WindowState.Minimized;
                    }
                }));
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == App.WM_SHOWME)
            {
                BringWindowToFront();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void RestoreWindowGeometry()
        {
            if (IniManager.WindowWidth >= 400) Width = IniManager.WindowWidth;
            if (IniManager.WindowHeight >= 500) Height = IniManager.WindowHeight;

            if (IniManager.WindowLeft >= 0 && IniManager.WindowTop >= 0)
            {
                // Ekran sınırları içinde mi kontrol et
                double virtualLeft = SystemParameters.VirtualScreenLeft;
                double virtualTop = SystemParameters.VirtualScreenTop;
                double virtualWidth = SystemParameters.VirtualScreenWidth;
                double virtualHeight = SystemParameters.VirtualScreenHeight;

                if (IniManager.WindowLeft >= virtualLeft &&
                    IniManager.WindowLeft + 200 <= virtualLeft + virtualWidth &&
                    IniManager.WindowTop >= virtualTop &&
                    IniManager.WindowTop + 150 <= virtualTop + virtualHeight)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Left = IniManager.WindowLeft;
                    Top = IniManager.WindowTop;
                }
            }
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                IniManager.WindowLeft = Left;
                IniManager.WindowTop = Top;
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                IniManager.WindowWidth = ActualWidth;
                IniManager.WindowHeight = ActualHeight;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            IniManager.Save();

            if (IniManager.EnableSystemTray && IniManager.MinimizeToTray && !_isExplicitExit)
            {
                e.Cancel = true;
                Hide();
                if (_notifyIcon != null && !_trayBalloonShownOnce)
                {
                    _trayBalloonShownOnce = true;
                    _notifyIcon.ShowBalloonTip(1500, "FakeLord", 
                        IniManager.Language == "EN" ? "FakeLord is running in the system tray." : "FakeLord sistem tepsisinde arka planda çalışmaya devam ediyor.", 
                        Forms.ToolTipIcon.Info);
                }
            }
            else
            {
                DisposeSystemTray();
            }
        }

        private void LoadInitialGames()
        {
            _allGames = GameDatabase.LoadCachedGames();
            _currentViewGames = _allGames;
            _activeTab = "All";
            UpdateTabButtonsUI();
            UpdateGameCountBadge();
            GamesVisibleList.ItemsSource = _currentViewGames;
            InitializeAlphaJumpBar();
            UpdateAlphaJumpAvailability();

            // Son seçilen oyun veya ilk oyun (tüm oyunlarda)
            var last = _allGames.FirstOrDefault(g => g.ExeName.Equals(IniManager.LastGame, StringComparison.OrdinalIgnoreCase))
                       ?? _allGames.FirstOrDefault();

            if (last != null)
            {
                SelectGame(last);
                GamesVisibleList.SelectedItem = last;
                GamesVisibleList.ScrollIntoView(last);
                UpdateActiveAlphaFromGame(last);
            }
        }

        private void UpdateGameCountBadge()
        {
            string unit = IniManager.Language == "EN" ? "Games" : "Oyun";
            GameCountBadge.Text = $"{_currentViewGames.Count} {unit}";
        }

        private void SelectGame(GameItem game)
        {
            _selectedGame = game;

            if (GamesVisibleList.SelectedItem != game)
            {
                GamesVisibleList.SelectedItem = game;
            }

            PreviewGameTitle.Text = game.Title;
            PreviewGameExe.Text = $"Exe: {game.ExeName}";
            
            if (_isPlaying)
            {
                PreviewStatus.Text = LocalizationManager.Get("PlayingStatus");
            }
            else
            {
                PreviewStatus.Text = LocalizationManager.Get("ReadyToPlay");
            }

            UpdateFavoriteToggleButton();
            LoadGameImage(game.ImageUrl);
        }

        private async void LoadGameImage(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    PreviewGameIcon.Source = null;
                    PreviewBadgeFallback.Visibility = Visibility.Visible;
                    return;
                }

                // Yerel dosya kontrolü
                string localPath = url;
                if (!Path.IsPathRooted(localPath))
                {
                    localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, url);
                }

                if (File.Exists(localPath))
                {
                    var localBitmap = new BitmapImage();
                    localBitmap.BeginInit();
                    localBitmap.UriSource = new Uri(localPath, UriKind.Absolute);
                    localBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    localBitmap.EndInit();
                    PreviewGameIcon.Source = localBitmap;
                    PreviewBadgeFallback.Visibility = Visibility.Collapsed;
                    return;
                }

                // Discord CDN URL'leri User-Agent olmadan 403 Forbidden verdiği için HttpClient ile önbelleğe alıp yerelden yüklüyoruz
                if (url.Contains("cdn.discordapp.com/app-icons/"))
                {
                    string safeName = Path.GetFileName(new Uri(url).AbsolutePath);
                    string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Cache", "Icons");
                    string cachePath = Path.Combine(cacheDir, safeName);

                    if (!File.Exists(cachePath))
                    {
                        string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache", "Icons", safeName);
                        if (File.Exists(altPath)) cachePath = altPath;
                    }

                    if (!File.Exists(cachePath))
                    {
                        await Task.Run(async () =>
                        {
                            try
                            {
                                using var client = new HttpClient();
                                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                                byte[] bytes = await client.GetByteArrayAsync(url);
                                Directory.CreateDirectory(cacheDir);
                                await File.WriteAllBytesAsync(cachePath, bytes);
                            }
                            catch { }
                        });
                    }

                    if (File.Exists(cachePath))
                    {
                        var cachedBmp = new BitmapImage();
                        cachedBmp.BeginInit();
                        cachedBmp.UriSource = new Uri(cachePath, UriKind.Absolute);
                        cachedBmp.CacheOption = BitmapCacheOption.OnLoad;
                        cachedBmp.EndInit();
                        PreviewGameIcon.Source = cachedBmp;
                        PreviewBadgeFallback.Visibility = Visibility.Collapsed;
                        return;
                    }
                }

                // Steam CDN veya genel Web URL yükleme
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.DownloadCompleted += (s, e) =>
                {
                    PreviewBadgeFallback.Visibility = Visibility.Collapsed;
                };
                bitmap.DownloadFailed += (s, e) =>
                {
                    PreviewBadgeFallback.Visibility = Visibility.Visible;
                    PreviewGameIcon.Source = null;
                };
                bitmap.EndInit();

                PreviewGameIcon.Source = bitmap;
                PreviewBadgeFallback.Visibility = Visibility.Collapsed;
            }
            catch
            {
                PreviewGameIcon.Source = null;
                PreviewBadgeFallback.Visibility = Visibility.Visible;
            }
        }

        #region Favoriler Sistemi
        private void RenderFavoritesBar()
        {
            FavoritesPanel.Children.Clear();

            foreach (var exeName in IniManager.Favorites)
            {
                var game = _allGames.FirstOrDefault(g => g.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase) ||
                                                         g.DisplayExeName.Equals(Path.GetFileName(exeName), StringComparison.OrdinalIgnoreCase) ||
                                                         (exeName.Contains("cs", StringComparison.OrdinalIgnoreCase) && g.Title.Contains("Counter-Strike", StringComparison.OrdinalIgnoreCase)) ||
                                                         (exeName.Contains("lol", StringComparison.OrdinalIgnoreCase) && g.Title.Contains("League of Legends", StringComparison.OrdinalIgnoreCase)) ||
                                                         (exeName.Contains("valorant", StringComparison.OrdinalIgnoreCase) && g.Title.Contains("VALORANT", StringComparison.OrdinalIgnoreCase)))
                           ?? GameDatabase.GetDefaultPopularGames().FirstOrDefault(g => g.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase) ||
                                                                                       g.DisplayExeName.Equals(Path.GetFileName(exeName), StringComparison.OrdinalIgnoreCase))
                           ?? new GameItem { ExeName = exeName, Title = exeName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase) };

                var btn = new Button
                {
                    Tag = game,
                    Margin = new Thickness(0, 0, 6, 0),
                    Padding = new Thickness(8, 4, 10, 4),
                    FontSize = 11,
                    FontWeight = FontWeights.Medium,
                    Cursor = Cursors.Hand,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#181E2B")),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748")),
                    BorderThickness = new Thickness(1)
                };

                // Oyun ikonu ve başlığı içeren yatay panel
                var contentPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

                bool iconAdded = false;
                if (!string.IsNullOrWhiteSpace(game.ImageUrl))
                {
                    try
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(game.ImageUrl, UriKind.RelativeOrAbsolute);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        var iconImg = new Image
                        {
                            Source = bmp,
                            Width = 16,
                            Height = 16,
                            Stretch = Stretch.Uniform,
                            Margin = new Thickness(0, 0, 6, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        RenderOptions.SetBitmapScalingMode(iconImg, BitmapScalingMode.HighQuality);
                        contentPanel.Children.Add(iconImg);
                        iconAdded = true;
                    }
                    catch { }
                }

                if (!iconAdded)
                {
                    var iconText = new TextBlock
                    {
                        Text = "🎮",
                        FontSize = 11,
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    contentPanel.Children.Add(iconText);
                }

                var titleText = new TextBlock
                {
                    Text = game.Title,
                    FontSize = 11,
                    FontWeight = FontWeights.Medium,
                    VerticalAlignment = VerticalAlignment.Center
                };
                contentPanel.Children.Add(titleText);
                btn.Content = contentPanel;

                var template = new ControlTemplate(typeof(Button));
                var borderFactory = new FrameworkElementFactory(typeof(Border));
                borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
                borderFactory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                borderFactory.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                borderFactory.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                borderFactory.SetBinding(Border.PaddingProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });

                var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
                borderFactory.AppendChild(contentFactory);
                template.VisualTree = borderFactory;
                btn.Template = template;

                btn.Click += (s, e) =>
                {
                    if (s is Button b && b.Tag is GameItem clickedGame)
                    {
                        var fullGame = _allGames.FirstOrDefault(g => g.ExeName.Equals(clickedGame.ExeName, StringComparison.OrdinalIgnoreCase))
                                       ?? GameDatabase.GetDefaultPopularGames().FirstOrDefault(g => g.ExeName.Equals(clickedGame.ExeName, StringComparison.OrdinalIgnoreCase))
                                       ?? clickedGame;
                        SelectGame(fullGame);
                        StartSelectedGame();
                    }
                };

                FavoritesPanel.Children.Add(btn);
            }

            UpdateFavoriteToggleButton();

            // Favoriler listesinin tam ve kesintisiz sığması için pencere genişliğini dinamik ayarla
            try
            {
                FavoritesPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double favWidth = FavoritesPanel.DesiredSize.Width;
                double neededWindowWidth = Math.Max(570, Math.Ceiling(favWidth + 240));

                MinWidth = neededWindowWidth;
                if (Width < neededWindowWidth)
                {
                    Width = neededWindowWidth;
                    IniManager.WindowWidth = neededWindowWidth;
                }
            }
            catch { }
        }

        private void UpdateFavoriteToggleButton()
        {
            if (_selectedGame == null) return;

            bool isFav = IniManager.Favorites.Any(f => f.Equals(_selectedGame.ExeName, StringComparison.OrdinalIgnoreCase));
            BtnToggleFav.Content = isFav ? LocalizationManager.Get("RemoveFav") : LocalizationManager.Get("AddFav");
            BtnToggleFav.Foreground = isFav
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC"));
        }

        private void StartSelectedGame()
        {
            if (_selectedGame == null) return;
            BtnStart_Click(this, new RoutedEventArgs());
        }

        private void BtnToggleFav_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGame == null) return;

            string exe = _selectedGame.ExeName;
            if (IniManager.Favorites.Any(f => f.Equals(exe, StringComparison.OrdinalIgnoreCase)))
            {
                IniManager.Favorites.RemoveAll(f => f.Equals(exe, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                IniManager.Favorites.Add(exe);
            }

            IniManager.Save();
            RenderFavoritesBar();
        }
        #endregion

        #region Steam Entegrasyonları (Top 100, Yüklü Oyunlar, Özel AppID)
        private void UpdateTabButtonsUI()
        {
            var currentTheme = AvailableThemes.FirstOrDefault(t => t.Key.Equals(IniManager.Theme, StringComparison.OrdinalIgnoreCase)) ?? AvailableThemes[0];
            var activeBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentTheme.AccentCol));
            var activeFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentTheme.TextAccent));
            var inactiveBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentTheme.InnerBg));
            var inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

            BtnTabAll.Background = _activeTab == "All" ? activeBg : inactiveBg;
            BtnTabAll.Foreground = _activeTab == "All" ? activeFg : inactiveFg;

            BtnTabSteamTop.Background = _activeTab == "SteamTop" ? activeBg : inactiveBg;
            BtnTabSteamTop.Foreground = _activeTab == "SteamTop" ? activeFg : inactiveFg;

            BtnTabSteamInstalled.Background = _activeTab == "SteamInstalled" ? activeBg : inactiveBg;
            BtnTabSteamInstalled.Foreground = _activeTab == "SteamInstalled" ? activeFg : inactiveFg;
        }

        private void FilterGamesList(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                GamesVisibleList.ItemsSource = _currentViewGames;
            }
            else
            {
                var matches = _currentViewGames
                    .Where(g => MatchesGame(g, query))
                    .ToList();

                // Eğer aktif sekme Yüklü Oyunlar DEĞİLSE ve eşleşme çıkmadıysa, tüm oyun kataloğunda da ara
                if (matches.Count == 0 && _allGames.Count > 0 && _activeTab != "SteamInstalled")
                {
                    matches = _allGames
                        .Where(g => MatchesGame(g, query))
                        .ToList();
                }

                GamesVisibleList.ItemsSource = matches;
                if (matches.Count > 0 && (GamesVisibleList.SelectedItem == null || !matches.Contains((GameItem)GamesVisibleList.SelectedItem)))
                {
                    GamesVisibleList.SelectedIndex = 0;
                }
            }
            UpdateAlphaJumpAvailability();
        }

        private static bool MatchesGame(GameItem g, string query)
        {
            if (g.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
            if (g.ExeName.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.IsNullOrEmpty(g.SteamAppId) && g.SteamAppId.Contains(query, StringComparison.OrdinalIgnoreCase)) return true;

            // Kısaltmalar ve akıllı takma adlar (LoL, Valo, CS, GTA, MC vb.)
            string q = query.Trim().ToLowerInvariant();
            if ((q == "lol" || q == "league") && g.Title.Contains("League of Legends", StringComparison.OrdinalIgnoreCase)) return true;
            if ((q == "valo" || q == "val") && g.Title.Contains("VALORANT", StringComparison.OrdinalIgnoreCase)) return true;
            if ((q == "cs" || q == "cs2" || q == "csgo") && (g.Title.Contains("Counter-Strike", StringComparison.OrdinalIgnoreCase) || g.ExeName.Contains("cs2"))) return true;
            if ((q == "gta" || q == "gta5" || q == "gtav") && g.Title.Contains("Grand Theft Auto", StringComparison.OrdinalIgnoreCase)) return true;
            if ((q == "mc" || q == "mine") && g.Title.Contains("Minecraft", StringComparison.OrdinalIgnoreCase)) return true;
            if (q == "rdr" && g.Title.Contains("Red Dead Redemption", StringComparison.OrdinalIgnoreCase)) return true;
            if (q == "r6" && g.Title.Contains("Rainbow Six", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }



        private void BtnTabAll_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "All";
            UpdateTabButtonsUI();
            _currentViewGames = _allGames;
            UpdateGameCountBadge();

            _isUpdatingSearchText = true;
            SearchBox.Text = "";
            _isUpdatingSearchText = false;

            FilterGamesList("");
            if (_currentViewGames.Count > 0)
            {
                SelectGame(_currentViewGames[0]);
                GamesVisibleList.SelectedIndex = 0;
                GamesVisibleList.ScrollIntoView(_currentViewGames[0]);
            }
            StatusMessage.Text = LocalizationManager.Get("ReadyStatusMsg");
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
        }

        private async void BtnTabSteamTop_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "SteamTop";
            UpdateTabButtonsUI();
            StatusMessage.Text = "⏳ Steam Top 100 oyun listesi canlı çekiliyor...";
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5865F2"));

            var topGames = await Task.Run(() => SteamIntegrationManager.FetchSteamTop100Async(_allGames));
            if (topGames.Count > 0)
            {
                _currentViewGames = topGames;
                UpdateGameCountBadge();

                _isUpdatingSearchText = true;
                SearchBox.Text = "";
                _isUpdatingSearchText = false;

                FilterGamesList("");
                SelectGame(topGames[0]);
                GamesVisibleList.SelectedIndex = 0;
                GamesVisibleList.ScrollIntoView(topGames[0]);

                StatusMessage.Text = $"● Steam'de en çok oynanan {topGames.Count} oyun yüklendi. İstediğinizi seçip hemen oynayabilirsiniz!";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            else
            {
                StatusMessage.Text = "❌ Steam Top 100 listesi alınamadı.";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4245"));
            }
        }

        private void BtnTabSteamInstalled_Click(object sender, RoutedEventArgs e)
        {
            _activeTab = "SteamInstalled";
            UpdateTabButtonsUI();
            StatusMessage.Text = "⏳ Bilgisayarınızdaki Steam kütüphaneleri taranıyor...";
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5865F2"));

            var installed = SteamIntegrationManager.DetectInstalledSteamGames(_allGames);
            if (installed.Count > 0)
            {
                _currentViewGames = installed;
                UpdateGameCountBadge();

                _isUpdatingSearchText = true;
                SearchBox.Text = "";
                _isUpdatingSearchText = false;

                FilterGamesList("");
                SelectGame(installed[0]);
                GamesVisibleList.SelectedIndex = 0;
                GamesVisibleList.ScrollIntoView(installed[0]);

                StatusMessage.Text = $"● Bilgisayarınızda yüklü {installed.Count} Steam oyunu bulundu!";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            else
            {
                StatusMessage.Text = "❌ Yüklü Steam oyunu bulunamadı veya Steam yolu tespit edilemedi.";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4245"));
            }
        }

        private async void BtnAddSteamApp_Click(object sender, RoutedEventArgs e)
        {
            string input = SearchBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input) || (!int.TryParse(input, out _) && !input.Contains("app/")))
            {
                StatusMessage.Text = "ℹ️ Steam AppID eklemek için yukarıdaki arama kutusuna AppID yazın (Örn: 730, 271590, 1091500) ve bu butona tıklayın.";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
                SearchBox.Focus();
                return;
            }

            StatusMessage.Text = $"⏳ Steam AppID: {input} bilgileri mağazadan alınıyor...";
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5865F2"));

            var customGame = await SteamIntegrationManager.FetchGameByAppIdOrUrlAsync(input, _allGames);
            if (customGame != null)
            {
                _allGames.Insert(0, customGame);
                _currentViewGames = _allGames;
                UpdateGameCountBadge();

                _isUpdatingSearchText = true;
                SearchBox.Text = "";
                _isUpdatingSearchText = false;

                FilterGamesList("");
                SelectGame(customGame);
                GamesVisibleList.SelectedItem = customGame;
                GamesVisibleList.ScrollIntoView(customGame);

                StatusMessage.Text = LocalizationManager.Get("SteamAddSuccess", customGame.Title);
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            else
            {
                StatusMessage.Text = LocalizationManager.Get("SteamAddError");
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4245"));
            }
        }
        #endregion

        #region Tema ve Dil Sistemi
        private void ApplyLanguage(string lang)
        {
            IniManager.Language = lang;
            IniManager.Save();

            TxtSubtitle.Text = LocalizationManager.Get("AppSubtitle");
            TxtLiveCardHeader.Text = LocalizationManager.Get("LiveCardTitle");
            TxtPlayingTitle.Text = LocalizationManager.Get("PlayingAGame");
            BtnStart.Content = LocalizationManager.Get("PlayOnDiscord");
            BtnStop.Content = LocalizationManager.Get("StopGame");
            TxtFavLabel.Text = LocalizationManager.Get("FavTitle");
            BtnTabAll.Content = LocalizationManager.Get("TabAll");
            BtnTabSteamTop.Content = LocalizationManager.Get("TabSteamTop");
            BtnTabSteamInstalled.Content = LocalizationManager.Get("TabSteamInstalled");
            BtnAddSteamApp.Content = LocalizationManager.Get("BtnAddSteamApp");

            BtnAbout.ToolTip = LocalizationManager.Get("AboutBtnTooltip");
            BtnRefreshDiscord.ToolTip = LocalizationManager.Get("TooltipRefreshDiscord");
            BtnSettings.ToolTip = LocalizationManager.Get("TooltipSettings");
            BtnAddSteamApp.ToolTip = LocalizationManager.Get("TooltipAddSteam");
            TxtAboutDeveloper.Text = LocalizationManager.Get("AboutDeveloper");
            TxtAboutDesc.Text = LocalizationManager.Get("AboutDesc");
            TxtWebsiteTitle.Text = LocalizationManager.Get("AboutWebsiteTitle");
            TxtWebsiteDesc.Text = LocalizationManager.Get("AboutWebsiteDesc");
            TxtGithubTitle.Text = LocalizationManager.Get("AboutGithubTitle");
            TxtGithubDesc.Text = LocalizationManager.Get("AboutGithubDesc");
            TxtIssuesTitle.Text = LocalizationManager.Get("AboutIssuesTitle");
            TxtIssuesDesc.Text = LocalizationManager.Get("AboutIssuesDesc");
            TxtEmailTitle.Text = LocalizationManager.Get("AboutEmailTitle");
            TxtEmailDesc.Text = LocalizationManager.Get("AboutEmailDesc");
            BtnAboutClose.Content = LocalizationManager.Get("AboutClose");

            if (!_isPlaying)
            {
                PreviewStatus.Text = LocalizationManager.Get("ReadyToPlay");
                StatusMessage.Text = LocalizationManager.Get("ReadyStatusMsg");
            }
            else
            {
                PreviewStatus.Text = LocalizationManager.Get("PlayingStatus");
                if (_selectedGame != null)
                {
                    StatusMessage.Text = LocalizationManager.Get("ActiveStatusMsg", _selectedGame.Title);
                }
            }

            UpdateFavoriteToggleButton();
            UpdateGameCountBadge();
            PopulateThemeComboBox();
            PopulateSettingsThemeComboBox();
            PopulateSettingsLangComboBox();
            UpdateSettingsTexts();
        }

        private void PopulateThemeComboBox()
        {
            _isPopulatingThemes = true;
            try
            {
                ComboTheme.Items.Clear();
                bool isEn = IniManager.Language == "EN";

                int selectIdx = 0;
                for (int i = 0; i < AvailableThemes.Count; i++)
                {
                    var th = AvailableThemes[i];
                    var item = new ComboBoxItem
                    {
                        Content = isEn ? th.EnName : th.TrName,
                        Tag = th.Key,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")),
                        Template = (ControlTemplate)Resources["ThemeComboItemTemplate"]
                    };

                    ComboTheme.Items.Add(item);

                    if (th.Key.Equals(IniManager.Theme, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIdx = i;
                    }
                }

                ComboTheme.SelectedIndex = selectIdx;
            }
            finally
            {
                _isPopulatingThemes = false;
            }
        }

        private void PopulateSettingsThemeComboBox()
        {
            _isPopulatingSettingsThemes = true;
            try
            {
                ComboSettingsTheme.Items.Clear();
                bool isEn = IniManager.Language == "EN";

                int selectIdx = 0;
                for (int i = 0; i < AvailableThemes.Count; i++)
                {
                    var th = AvailableThemes[i];
                    var item = new ComboBoxItem
                    {
                        Content = isEn ? th.EnName : th.TrName,
                        Tag = th.Key,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")),
                        Template = (ControlTemplate)Resources["ThemeComboItemTemplate"]
                    };

                    ComboSettingsTheme.Items.Add(item);

                    if (th.Key.Equals(IniManager.Theme, StringComparison.OrdinalIgnoreCase))
                    {
                        selectIdx = i;
                    }
                }

                ComboSettingsTheme.SelectedIndex = selectIdx;
            }
            finally
            {
                _isPopulatingSettingsThemes = false;
            }
        }

        private void SyncSettingsThemeSelection(string themeKey)
        {
            if (ComboSettingsTheme == null) return;
            _isPopulatingSettingsThemes = true;
            try
            {
                for (int i = 0; i < ComboSettingsTheme.Items.Count; i++)
                {
                    if (ComboSettingsTheme.Items[i] is ComboBoxItem item && item.Tag is string key && key.Equals(themeKey, StringComparison.OrdinalIgnoreCase))
                    {
                        ComboSettingsTheme.SelectedIndex = i;
                        break;
                    }
                }
            }
            finally
            {
                _isPopulatingSettingsThemes = false;
            }
        }

        private void PopulateSettingsLangComboBox()
        {
            if (ComboSettingsLang == null) return;
            ComboSettingsLang.Items.Clear();

            // Türkçe ComboBoxItem (Gerçek Bayrak Görseli)
            var trPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            try
            {
                var trBmp = new BitmapImage(new Uri("pack://application:,,,/flag_tr.png", UriKind.RelativeOrAbsolute));
                var trImg = new Image { Source = trBmp, Width = 18, Height = 13, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 7, 0) };
                RenderOptions.SetBitmapScalingMode(trImg, BitmapScalingMode.HighQuality);
                trPanel.Children.Add(trImg);
            }
            catch { }
            trPanel.Children.Add(new TextBlock { Text = "Türkçe", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")), VerticalAlignment = VerticalAlignment.Center });

            var trItem = new ComboBoxItem
            {
                Content = trPanel,
                Tag = "TR",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")),
                Template = (ControlTemplate)Resources["ThemeComboItemTemplate"]
            };

            // English ComboBoxItem (Gerçek Bayrak Görseli)
            var enPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            try
            {
                var enBmp = new BitmapImage(new Uri("pack://application:,,,/flag_gb.png", UriKind.RelativeOrAbsolute));
                var enImg = new Image { Source = enBmp, Width = 18, Height = 13, Stretch = Stretch.Uniform, Margin = new Thickness(0, 0, 7, 0) };
                RenderOptions.SetBitmapScalingMode(enImg, BitmapScalingMode.HighQuality);
                enPanel.Children.Add(enImg);
            }
            catch { }
            enPanel.Children.Add(new TextBlock { Text = "English", Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")), VerticalAlignment = VerticalAlignment.Center });

            var enItem = new ComboBoxItem
            {
                Content = enPanel,
                Tag = "EN",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F6FC")),
                Template = (ControlTemplate)Resources["ThemeComboItemTemplate"]
            };

            ComboSettingsLang.Items.Add(trItem);
            ComboSettingsLang.Items.Add(enItem);

            ComboSettingsLang.SelectedIndex = IniManager.Language.Equals("EN", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        private void ComboSettingsTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPopulatingSettingsThemes) return;
            if (ComboSettingsTheme.SelectedItem is ComboBoxItem item && item.Tag is string key)
            {
                _isPopulatingThemes = true;
                for (int i = 0; i < ComboTheme.Items.Count; i++)
                {
                    if (ComboTheme.Items[i] is ComboBoxItem ci && ci.Tag is string tk && tk.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        ComboTheme.SelectedIndex = i;
                        break;
                    }
                }
                _isPopulatingThemes = false;

                ApplyTheme(key);
            }
        }

        private void ComboTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isPopulatingThemes) return;
            if (ComboTheme.SelectedItem is ComboBoxItem item && item.Tag is string key)
            {
                ApplyTheme(key);
            }
        }

        private void ApplyTheme(string themeName)
        {
            // Eski tema anahtarlarını yeni Theme Factory koleksiyonuna güvenle eşle
            string normalizedKey = themeName switch
            {
                "ObsidianAbyss" => "TechInnovation",
                "AbyssalOcean" => "OceanDepths",
                "VaporwaveSunset" => "SunsetBoulevard",
                "MatrixEmerald" => "ForestCanopy",
                "SolarFlare" => "GoldenHour",
                "AmethystNight" => "MidnightGalaxy",
                _ => themeName
            };

            var theme = AvailableThemes.FirstOrDefault(t => t.Key.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase))
                        ?? AvailableThemes[0];

            IniManager.Theme = theme.Key;
            IniManager.Save();

            var winBg = (Color)ColorConverter.ConvertFromString(theme.WinBg);
            var cardBg = (Color)ColorConverter.ConvertFromString(theme.CardBg);
            var innerBg = (Color)ColorConverter.ConvertFromString(theme.InnerBg);
            var borderCol = (Color)ColorConverter.ConvertFromString(theme.BorderCol);
            var accentCol = (Color)ColorConverter.ConvertFromString(theme.AccentCol);
            var accentHover = (Color)ColorConverter.ConvertFromString(theme.AccentHover);
            var textAccent = (Color)ColorConverter.ConvertFromString(theme.TextAccent);

            var accentBrush = new SolidColorBrush(accentCol);
            var accentHoverBrush = new SolidColorBrush(accentHover);
            var textAccentBrush = new SolidColorBrush(textAccent);
            var winBgBrush = new SolidColorBrush(winBg);
            var cardBgBrush = new SolidColorBrush(cardBg);
            var innerBgBrush = new SolidColorBrush(innerBg);
            var borderBrush = new SolidColorBrush(borderCol);

            // Dinamik Kaynakları (DynamicResource) Güncelle
            Resources["ThemeAccentBrush"] = accentBrush;
            Resources["ThemeAccentHoverBrush"] = accentHoverBrush;
            Resources["ThemeTextAccentBrush"] = textAccentBrush;
            Resources["ThemeWinBgBrush"] = winBgBrush;
            Resources["ThemeCardBgBrush"] = cardBgBrush;
            Resources["ThemeInnerBgBrush"] = innerBgBrush;
            Resources["ThemeBorderBrush"] = borderBrush;

            RootWindow.Background = winBgBrush;
            MainCardBorder.Background = cardBgBrush;
            MainCardBorder.BorderBrush = borderBrush;

            FavBorder.Background = cardBgBrush;
            FavBorder.BorderBrush = borderBrush;

            SearchBoxBorder.Background = innerBgBrush;
            SearchBoxBorder.BorderBrush = borderBrush;

            GameListBorder.Background = cardBgBrush;
            GameListBorder.BorderBrush = borderBrush;

            InnerGameCard.Background = innerBgBrush;
            InnerGameCard.BorderBrush = borderBrush;

            ComboTheme.Background = innerBgBrush;
            ComboTheme.BorderBrush = borderBrush;

            if (ComboSettingsTheme != null)
            {
                ComboSettingsTheme.Background = innerBgBrush;
                ComboSettingsTheme.BorderBrush = borderBrush;
            }

            if (ComboSettingsLang != null)
            {
                ComboSettingsLang.Background = innerBgBrush;
                ComboSettingsLang.BorderBrush = borderBrush;
            }

            AboutCardBorder.Background = cardBgBrush;
            AboutCardBorder.BorderBrush = borderBrush;

            SettingsCardBorder.Background = cardBgBrush;
            SettingsCardBorder.BorderBrush = borderBrush;

            FooterBorder.Background = winBgBrush;
            FooterBorder.BorderBrush = borderBrush;

            if (AlphaJumpBorder != null)
            {
                AlphaJumpBorder.Background = innerBgBrush;
                AlphaJumpBorder.BorderBrush = borderBrush;
            }

            // 🎯 Vurgu Öğelerini Temayla Senkronize Et
            BtnStart.Background = accentBrush;
            BtnStart.Foreground = textAccentBrush;
            TxtVersionBadge.Foreground = accentBrush;
            VersionBadgeBorder.BorderBrush = borderBrush;
            GameCountBadge.Foreground = accentBrush;

            UpdateTabButtonsUI();
            SyncSettingsThemeSelection(theme.Key);
            UpdateAlphaJumpAvailability();
        }
        #endregion

        #region Arama ve Liste Etkileşimi
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSearchText) return;

            string query = SearchBox.Text.Trim();

            // Steam Top 100 sekmesindeyken arama yapıldığında otomatik olarak Tüm Oyunlar filtresine geç
            if (!string.IsNullOrWhiteSpace(query) && _activeTab == "SteamTop")
            {
                _activeTab = "All";
                _currentViewGames = _allGames;
                UpdateTabButtonsUI();
                UpdateGameCountBadge();
            }

            FilterGamesList(query);
        }

        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Down && GamesVisibleList.Items.Count > 0)
            {
                GamesVisibleList.Focus();
                if (GamesVisibleList.SelectedIndex < 0) GamesVisibleList.SelectedIndex = 0;
                e.Handled = true;
            }
        }

        private void GamesVisibleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesVisibleList.SelectedItem is GameItem selected)
            {
                SelectGame(selected);
                UpdateActiveAlphaFromGame(selected);
            }
        }

        private void GamesVisibleList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GamesVisibleList.SelectedItem is GameItem selected)
            {
                SelectGame(selected);
                BtnStart_Click(sender, e);
            }
        }
        #endregion

        #region A-Z Hızlı Harf Şeridi (Alfabetik Quick Jump)
        private void InitializeAlphaJumpBar()
        {
            if (_alphaButtons.Count > 0 || AlphaJumpGrid == null) return;

            AlphaJumpGrid.Children.Clear();
            char[] chars = new char[]
            {
                '#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
                'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };

            foreach (char c in chars)
            {
                var btn = new Button
                {
                    Content = c.ToString(),
                    Tag = c,
                    Style = (Style)Resources["AlphaJumpButtonStyle"],
                    ToolTip = c == '#' ? "# (0-9 & Semboller)" : $"'{c}' harfine git"
                };
                btn.Click += (s, e) => JumpToLetter(c);
                _alphaButtons[c] = btn;
                AlphaJumpGrid.Children.Add(btn);
            }
        }

        private void JumpToLetter(char letter)
        {
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                _isUpdatingSearchText = true;
                SearchBox.Text = "";
                _isUpdatingSearchText = false;
                FilterGamesList("");
            }

            var items = (GamesVisibleList.ItemsSource as IEnumerable<GameItem>)?.ToList() ?? _currentViewGames;
            if (items == null || items.Count == 0) return;

            GameItem? target = null;
            char targetUpper = char.ToUpperInvariant(letter);

            if (targetUpper == '#')
            {
                target = items.FirstOrDefault(g => IsSpecialOrDigit(g.Title));
            }
            else
            {
                target = items.FirstOrDefault(g => GetNormalizedFirstChar(g.Title) == targetUpper);
            }

            if (target != null)
            {
                GamesVisibleList.SelectedItem = target;
                GamesVisibleList.ScrollIntoView(target);
                SelectGame(target);
                _currentActiveAlphaChar = targetUpper;
                UpdateAlphaJumpAvailability();

                string msg = IniManager.Language == "EN"
                    ? $"⚡ Jumped to '{letter}': {target.Title}"
                    : $"⚡ '{letter}' harfine gidildi: {target.Title}";
                StatusMessage.Text = msg;
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));
            }
            else
            {
                _currentActiveAlphaChar = null;
                UpdateAlphaJumpAvailability();

                string msg = IniManager.Language == "EN"
                    ? $"ℹ No games found starting with '{letter}'."
                    : $"ℹ '{letter}' ile başlayan oyun bulunamadı.";
                StatusMessage.Text = msg;
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
            }
        }

        private void UpdateAlphaJumpAvailability()
        {
            if (_alphaButtons.Count == 0) return;

            var items = (GamesVisibleList.ItemsSource as IEnumerable<GameItem>)?.ToList() ?? _currentViewGames;
            var presentLetters = new HashSet<char>();

            if (items != null)
            {
                foreach (var g in items)
                {
                    if (g != null && !string.IsNullOrWhiteSpace(g.Title))
                    {
                        presentLetters.Add(GetNormalizedFirstChar(g.Title));
                    }
                }
            }

            var currentTheme = AvailableThemes.FirstOrDefault(t => t.Key.Equals(IniManager.Theme, StringComparison.OrdinalIgnoreCase)) ?? AvailableThemes[0];
            var accentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentTheme.AccentCol));
            var textAccentBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(currentTheme.TextAccent));

            foreach (var kvp in _alphaButtons)
            {
                char c = kvp.Key;
                var btn = kvp.Value;
                bool exists = presentLetters.Contains(c);

                if (c == _currentActiveAlphaChar)
                {
                    btn.Background = accentBrush;
                    btn.Foreground = textAccentBrush;
                    btn.Opacity = 1.0;
                }
                else if (exists)
                {
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                    btn.Opacity = 1.0;
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
                    btn.Opacity = 0.40;
                }
            }
        }

        private void UpdateActiveAlphaFromGame(GameItem? game)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.Title))
            {
                _currentActiveAlphaChar = null;
            }
            else
            {
                _currentActiveAlphaChar = GetNormalizedFirstChar(game.Title);
            }
            UpdateAlphaJumpAvailability();
        }

        private static char GetNormalizedFirstChar(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return '#';
            string trimmed = title.TrimStart(' ', '"', '\'', '(', '[', '{', '!', '?', '#', '$', '%', '*', '+', '-');
            if (string.IsNullOrEmpty(trimmed)) return '#';
            char c = char.ToUpperInvariant(trimmed[0]);
            if (c == 'İ') return 'I';
            if (c >= 'A' && c <= 'Z') return c;
            return '#';
        }

        private static bool IsSpecialOrDigit(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return true;
            string trimmed = title.TrimStart(' ', '"', '\'', '(', '[', '{', '!', '?');
            if (string.IsNullOrEmpty(trimmed)) return true;
            char c = char.ToUpperInvariant(trimmed[0]);
            return !(c >= 'A' && c <= 'Z') && c != 'İ';
        }
        #endregion

        #region Oyun Başlatma / Durdurma / API
        private async void BtnRefreshDiscord_Click(object sender, RoutedEventArgs e)
        {
            BtnRefreshDiscord.IsEnabled = false;
            ShowToast(LocalizationManager.Get("ToastUpdatingAll"), "🔄");
            StatusMessage.Text = LocalizationManager.Get("CatalogUpdatingMsg");
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5865F2"));

            // 1. Discord Detectable 3.000+ oyun kataloğunu canlı çek
            var freshGames = await Task.Run(() => GameDatabase.FetchDiscordDetectableAsync());

            if (freshGames.Count > 0)
            {
                _allGames = freshGames;

                // 2. Aktif sekmeye göre görünümü canlı tazele
                if (_activeTab == "SteamTop")
                {
                    StatusMessage.Text = "⏳ Steam Top 100 listesi canlı güncelleniyor...";
                    var freshTop = await Task.Run(() => SteamIntegrationManager.FetchSteamTop100Async(_allGames));
                    if (freshTop.Count > 0) _currentViewGames = freshTop;
                }
                else if (_activeTab == "SteamInstalled")
                {
                    var freshInstalled = SteamIntegrationManager.DetectInstalledSteamGames(_allGames);
                    if (freshInstalled.Count > 0) _currentViewGames = freshInstalled;
                }
                else if (_activeTab == "Popular")
                {
                    _currentViewGames = GameDatabase.GetDefaultPopularGames();
                }
                else
                {
                    _currentViewGames = _allGames;
                }

                UpdateGameCountBadge();
                FilterGamesList(SearchBox.Text.Trim());

                if (GamesVisibleList.Items.Count > 0 && GamesVisibleList.SelectedItem == null)
                {
                    GamesVisibleList.SelectedIndex = 0;
                }

                StatusMessage.Text = LocalizationManager.Get("CatalogSuccessMsg", _allGames.Count);
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                RenderFavoritesBar();
                ShowToast(LocalizationManager.Get("ToastUpdatedAll"), "✅");
            }
            else
            {
                StatusMessage.Text = LocalizationManager.Get("CatalogErrorMsg");
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4245"));
            }

            BtnRefreshDiscord.IsEnabled = true;
        }

        private async Task AutoUpdateDiscordCatalogAsync()
        {
            try
            {
                var freshGames = await Task.Run(() => GameDatabase.FetchDiscordDetectableAsync());
                if (freshGames.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        _allGames = freshGames;
                        
                        if (_activeTab == "All")
                        {
                            _currentViewGames = _allGames;
                        }
                        else if (_activeTab == "Popular")
                        {
                            _currentViewGames = GameDatabase.GetDefaultPopularGames();
                        }

                        UpdateGameCountBadge();
                        FilterGamesList(SearchBox.Text.Trim());
                        RenderFavoritesBar();

                        // Açılışta güncellemenin yapıldığını kullanıcıya açıkça hissettir
                        StatusMessage.Text = LocalizationManager.Get("CatalogSuccessMsg", _allGames.Count);
                        StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                        ShowToast(LocalizationManager.Get("ToastUpdatedAll"), "🔄");

                        // Eğer seçili oyun varsa görsel ve detayları yenile
                        if (_selectedGame != null)
                        {
                            var updatedGame = _allGames.FirstOrDefault(g => g.ExeName.Equals(_selectedGame.ExeName, StringComparison.OrdinalIgnoreCase))
                                              ?? GameDatabase.GetDefaultPopularGames().FirstOrDefault(g => g.ExeName.Equals(_selectedGame.ExeName, StringComparison.OrdinalIgnoreCase));
                            if (updatedGame != null)
                            {
                                SelectGame(updatedGame);
                            }
                        }
                    });
                }
            }
            catch { }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedGame == null) return;

            string error;
            bool success = GhostProcessManager.StartGame(_selectedGame.ExeName, _selectedGame.Title, _selectedGame.ImageUrl, out error);
            if (success)
            {
                _isPlaying = true;
                IniManager.LastGame = _selectedGame.ExeName;
                IniManager.AddRecentGame(_selectedGame.ExeName);
                UpdateTrayContextMenu();

                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;

                StatusMessage.Text = LocalizationManager.Get("ActiveStatusMsg", _selectedGame.Title);
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                PreviewStatus.Text = LocalizationManager.Get("PlayingStatus");

                ShowToast(string.Format(LocalizationManager.Get("ToastGameStarted"), _selectedGame.Title), "🚀");

                // HaYTooL açıldığında FakelordUI ekrandan yok olsun (Yer değiştirme)
                this.Hide();
            }
            else
            {
                StatusMessage.Text = $"❌ {error}";
                StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED4245"));
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            GhostProcessManager.StopGame();
            _isPlaying = false;
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;

            StatusMessage.Text = LocalizationManager.Get("StoppedStatusMsg");
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
            PreviewStatus.Text = LocalizationManager.Get("StoppedStatus");
        }
        #endregion

        #region Hakkında (About) Penceresi Olayları
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutModalOverlay.Visibility = Visibility.Visible;
        }

        private void BtnCloseAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void AboutModalOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == AboutModalOverlay)
            {
                AboutModalOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void AboutCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (SettingsModalOverlay.Visibility == Visibility.Visible)
                {
                    SettingsModalOverlay.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                if (AboutModalOverlay.Visibility == Visibility.Visible)
                {
                    AboutModalOverlay.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                    return;
                }
                if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                    SearchBox.Text = "";
                    e.Handled = true;
                    return;
                }
            }

            // Metin kutusuna odaklı değilse klavyeden harfe veya rakama basıldığında doğrudan zıpla
            if (SettingsModalOverlay.Visibility != Visibility.Visible &&
                AboutModalOverlay.Visibility != Visibility.Visible &&
                !SearchBox.IsKeyboardFocused &&
                Keyboard.FocusedElement is not System.Windows.Controls.TextBox)
            {
                if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                    char letter = (char)('A' + (e.Key - Key.A));
                    JumpToLetter(letter);
                    e.Handled = true;
                    return;
                }
                else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                {
                    JumpToLetter('#');
                    e.Handled = true;
                    return;
                }
            }
        }

        #region Ayarlar (Settings) Modalı Olayları
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ChkEnableTray.IsChecked = IniManager.EnableSystemTray;
            ChkMinimizeToTray.IsChecked = IniManager.MinimizeToTray;
            ChkStartWithWindows.IsChecked = StartupManager.IsStartupEnabled();
            ChkStartMinimized.IsChecked = IniManager.StartMinimized;
            ChkAutoFetchDiscord.IsChecked = IniManager.AutoFetchDiscord;
            SyncSettingsThemeSelection(IniManager.Theme);
            PopulateSettingsLangComboBox();

            SettingsModalOverlay.Visibility = Visibility.Visible;
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void SettingsModalOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == SettingsModalOverlay)
            {
                SettingsModalOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingsCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            bool trayEnabled = ChkEnableTray.IsChecked == true;
            bool minToTray = ChkMinimizeToTray.IsChecked == true;
            bool startWithWin = ChkStartWithWindows.IsChecked == true;
            bool startMin = ChkStartMinimized.IsChecked == true;
            bool autoFetch = ChkAutoFetchDiscord.IsChecked == true;

            IniManager.EnableSystemTray = trayEnabled;
            IniManager.MinimizeToTray = minToTray;
            IniManager.StartWithWindows = startWithWin;
            IniManager.StartMinimized = startMin;
            IniManager.AutoFetchDiscord = autoFetch;

            if (ComboSettingsLang.SelectedItem is ComboBoxItem langItem && langItem.Tag is string langCode)
            {
                if (!langCode.Equals(IniManager.Language, StringComparison.OrdinalIgnoreCase))
                {
                    IniManager.Language = langCode;
                    ApplyLanguage(langCode);
                }
            }

            StartupManager.SetStartup(startWithWin);

            IniManager.Save();

            if (trayEnabled)
            {
                if (_notifyIcon == null)
                {
                    SetupSystemTray();
                }
                else
                {
                    _notifyIcon.Visible = true;
                    UpdateTrayContextMenu();
                }
            }
            else
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                }
            }

            SettingsModalOverlay.Visibility = Visibility.Collapsed;
            ShowToast(LocalizationManager.Get("SettingsSavedToast"), "⚙️");
        }

        private void UpdateSettingsTexts()
        {
            TxtSettingsHeaderTitle.Text = LocalizationManager.Get("SettingsTitle");
            TxtSettingsHeaderDesc.Text = LocalizationManager.Get("SettingsGeneral");
            TxtSettingsLangTitle.Text = LocalizationManager.Get("SettingsLangTitle");
            TxtSettingsLangDesc.Text = LocalizationManager.Get("SettingsLangDesc");
            TxtSettingsWebTitle.Text = LocalizationManager.Get("SettingsWebsiteTitle");
            TxtSettingsWebDesc.Text = LocalizationManager.Get("SettingsWebsiteDesc");
            TxtSettingsThemeTitle.Text = LocalizationManager.Get("SettingsAppearance");
            TxtSettingsThemeDesc.Text = LocalizationManager.Get("Theme_" + IniManager.Theme);

            TxtSettingsAutoFetchTitle.Text = LocalizationManager.Get("SettingsAutoFetchTitle");
            TxtSettingsAutoFetchDesc.Text = LocalizationManager.Get("SettingsAutoFetchDesc");

            TxtSettingsTrayTitle.Text = LocalizationManager.Get("SettingsEnableTray");
            TxtSettingsTrayDesc.Text = LocalizationManager.Get("SettingsEnableTrayDesc");

            TxtSettingsMinToTrayTitle.Text = LocalizationManager.Get("SettingsMinimizeToTray");
            TxtSettingsMinToTrayDesc.Text = LocalizationManager.Get("SettingsMinimizeToTrayDesc");

            TxtSettingsStartupTitle.Text = LocalizationManager.Get("SettingsStartWithWindows");
            TxtSettingsStartupDesc.Text = LocalizationManager.Get("SettingsStartWithWindowsDesc");

            TxtSettingsStartMinTitle.Text = LocalizationManager.Get("SettingsStartMinimized");
            TxtSettingsStartMinDesc.Text = LocalizationManager.Get("SettingsStartMinimizedDesc");

            BtnSettingsCancel.Content = LocalizationManager.Get("AboutClose");
            BtnSettingsSave.Content = LocalizationManager.Get("SettingsSave");

            UpdateTrayContextMenu();
        }
        #endregion

        #region Sistem Tepsisi (System Tray) Yönetimi
        private void SetupSystemTray()
        {
            if (!IniManager.EnableSystemTray) return;

            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    UpdateTrayContextMenu();
                    return;
                }

                _notifyIcon = new Forms.NotifyIcon();
                _notifyIcon.Text = "FakeLord - Discord Game Simulator";

                // İkon yükleme: Uygulamanın kendi derlenmiş ikonu
                try
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        _notifyIcon.Icon = Drawing.Icon.ExtractAssociatedIcon(exePath) ?? Drawing.SystemIcons.Application;
                    }
                    else
                    {
                        string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "app.ico");
                        if (!File.Exists(icoPath)) icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                        _notifyIcon.Icon = File.Exists(icoPath) ? new Drawing.Icon(icoPath) : Drawing.SystemIcons.Application;
                    }
                }
                catch
                {
                    _notifyIcon.Icon = Drawing.SystemIcons.Application;
                }

                _notifyIcon.DoubleClick += (s, e) =>
                {
                    BringWindowToFront();
                };

                UpdateTrayContextMenu();
                _notifyIcon.Visible = true;
            }
            catch { }
        }

        private void UpdateTrayContextMenu()
        {
            if (_notifyIcon == null) return;

            try
            {
                var menu = new Forms.ContextMenuStrip();
                menu.Renderer = new DarkTrayMenuRenderer();
                menu.ShowImageMargin = false;
                menu.ShowCheckMargin = false;
                menu.BackColor = Drawing.Color.FromArgb(14, 19, 31);
                menu.Font = new Drawing.Font("Segoe UI", 9.5f, Drawing.FontStyle.Regular);
                menu.Padding = new Forms.Padding(4, 5, 4, 5);

                // 1. Marka & Versiyon Başlığı (Tıklanırsa pencereyi öne getirir)
                var brandItem = new Forms.ToolStripMenuItem("🎮 FakeLord v1.6");
                brandItem.Tag = "header_brand";
                brandItem.Font = new Drawing.Font("Segoe UI", 9.5f, Drawing.FontStyle.Bold);
                brandItem.Padding = new Forms.Padding(12, 5, 12, 4);
                brandItem.ToolTipText = LocalizationManager.Get("TrayShow");
                brandItem.Click += (s, e) => BringWindowToFront();
                menu.Items.Add(brandItem);

                // 2. Canlı Oyun Durumu
                if (_isPlaying)
                {
                    string activeTitle = _selectedGame?.Title ?? GhostProcessManager.CurrentRunningGame ?? "Game";
                    var statusItem = new Forms.ToolStripMenuItem(LocalizationManager.Get("TrayStatusActive", activeTitle));
                    statusItem.Tag = "status_active";
                    statusItem.Font = new Drawing.Font("Segoe UI", 9.0f, Drawing.FontStyle.Bold);
                    statusItem.Padding = new Forms.Padding(12, 3, 12, 3);
                    statusItem.Click += (s, e) => BringWindowToFront();
                    menu.Items.Add(statusItem);

                    var stopItem = new Forms.ToolStripMenuItem(LocalizationManager.Get("TrayStopCurrent"));
                    stopItem.Tag = "stop_btn";
                    stopItem.Font = new Drawing.Font("Segoe UI", 9.0f, Drawing.FontStyle.Bold);
                    stopItem.Padding = new Forms.Padding(12, 4, 12, 4);
                    stopItem.Click += (s, e) =>
                    {
                        Dispatcher.Invoke(() => BtnStop_Click(this, new RoutedEventArgs()));
                    };
                    menu.Items.Add(stopItem);
                }
                else
                {
                    var idleItem = new Forms.ToolStripMenuItem(LocalizationManager.Get("TrayStatusIdle")) { Enabled = false };
                    idleItem.Font = new Drawing.Font("Segoe UI", 8.5f, Drawing.FontStyle.Regular);
                    idleItem.Padding = new Forms.Padding(12, 3, 12, 3);
                    menu.Items.Add(idleItem);
                }

                menu.Items.Add(new Forms.ToolStripSeparator());

                // 3. ⭐ Favori Oyunlar Alt Menüsü
                var favSubMenu = new Forms.ToolStripMenuItem($"⭐ {LocalizationManager.Get("TrayFavorites")}");
                favSubMenu.Padding = new Forms.Padding(12, 4, 12, 4);
                StyleDropDown(favSubMenu.DropDown, menu.Renderer);

                var favList = IniManager.Favorites.ToList();
                if (favList.Count > 0)
                {
                    foreach (var exe in favList)
                    {
                        var matchingGame = _allGames.FirstOrDefault(g => g.ExeName.Equals(exe, StringComparison.OrdinalIgnoreCase));
                        string gameTitle = matchingGame?.Title ?? exe.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

                        var gameItem = new Forms.ToolStripMenuItem($"▶ {gameTitle}");
                        gameItem.Padding = new Forms.Padding(12, 4, 12, 4);
                        gameItem.Click += (s, e) => Dispatcher.Invoke(() => LaunchGameFromTray(exe));
                        favSubMenu.DropDownItems.Add(gameItem);
                    }
                }
                else
                {
                    var emptyFav = new Forms.ToolStripMenuItem(LocalizationManager.Get("TrayNoFavorites")) { Enabled = false };
                    emptyFav.Padding = new Forms.Padding(12, 4, 12, 4);
                    favSubMenu.DropDownItems.Add(emptyFav);
                }
                menu.Items.Add(favSubMenu);

                // 4. 🕒 Son Oynanan Oyunlar Alt Menüsü
                var recentSubMenu = new Forms.ToolStripMenuItem($"🕒 {LocalizationManager.Get("TrayRecentGames")}");
                recentSubMenu.Padding = new Forms.Padding(12, 4, 12, 4);
                StyleDropDown(recentSubMenu.DropDown, menu.Renderer);

                var recentList = IniManager.RecentGames.Take(5).ToList();
                if (recentList.Count == 0 && !string.IsNullOrWhiteSpace(IniManager.LastGame))
                {
                    recentList.Add(IniManager.LastGame);
                }

                if (recentList.Count > 0)
                {
                    foreach (var exe in recentList)
                    {
                        var matchingGame = _allGames.FirstOrDefault(g => g.ExeName.Equals(exe, StringComparison.OrdinalIgnoreCase));
                        string gameTitle = matchingGame?.Title ?? exe.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

                        var gameItem = new Forms.ToolStripMenuItem($"▶ {gameTitle}");
                        gameItem.Padding = new Forms.Padding(12, 4, 12, 4);
                        gameItem.Click += (s, e) => Dispatcher.Invoke(() => LaunchGameFromTray(exe));
                        recentSubMenu.DropDownItems.Add(gameItem);
                    }
                }
                else
                {
                    var emptyRecent = new Forms.ToolStripMenuItem(LocalizationManager.Get("TrayNoRecent")) { Enabled = false };
                    emptyRecent.Padding = new Forms.Padding(12, 4, 12, 4);
                    recentSubMenu.DropDownItems.Add(emptyRecent);
                }
                menu.Items.Add(recentSubMenu);

                menu.Items.Add(new Forms.ToolStripSeparator());

                // 5. 🗔 Pencereyi Göster
                var showItem = new Forms.ToolStripMenuItem($"🗔 {LocalizationManager.Get("TrayShow")}");
                showItem.Font = new Drawing.Font("Segoe UI", 9.5f, Drawing.FontStyle.Bold);
                showItem.Padding = new Forms.Padding(12, 4, 12, 4);
                showItem.Click += (s, e) => BringWindowToFront();
                menu.Items.Add(showItem);

                // 6. ⚙️ Ayarlar
                var settingsItem = new Forms.ToolStripMenuItem($"⚙️ {LocalizationManager.Get("SettingsTitle")}");
                settingsItem.Padding = new Forms.Padding(12, 4, 12, 4);
                settingsItem.Click += (s, e) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        BringWindowToFront();
                        BtnSettings_Click(this, new RoutedEventArgs());
                    });
                };
                menu.Items.Add(settingsItem);

                // 7. 🌐 Resmi Web Sitesi
                var websiteItem = new Forms.ToolStripMenuItem($"🌐 {LocalizationManager.Get("TrayWebsite")}");
                websiteItem.Padding = new Forms.Padding(12, 4, 12, 4);
                websiteItem.Click += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = "https://haytool.online/FakeLord/", UseShellExecute = true });
                    }
                    catch { }
                };
                menu.Items.Add(websiteItem);

                menu.Items.Add(new Forms.ToolStripSeparator());

                // 8. ❌ Çıkış
                var exitItem = new Forms.ToolStripMenuItem($"❌ {LocalizationManager.Get("TrayExit")}");
                exitItem.Tag = "stop_btn";
                exitItem.Padding = new Forms.Padding(12, 5, 12, 5);
                exitItem.Click += (s, e) =>
                {
                    _isExplicitExit = true;
                    GhostProcessManager.StopGame();
                    DisposeSystemTray();
                    Application.Current.Shutdown();
                };
                menu.Items.Add(exitItem);

                _notifyIcon.ContextMenuStrip = menu;
            }
            catch { }
        }

        private static void StyleDropDown(Forms.ToolStripDropDown dropDown, Forms.ToolStripRenderer renderer)
        {
            if (dropDown == null) return;
            dropDown.Renderer = renderer;
            if (dropDown is Forms.ToolStripDropDownMenu menuDrop)
            {
                menuDrop.ShowImageMargin = false;
                menuDrop.ShowCheckMargin = false;
            }
            dropDown.BackColor = Drawing.Color.FromArgb(14, 19, 31);
            dropDown.Font = new Drawing.Font("Segoe UI", 9.5f, Drawing.FontStyle.Regular);
            dropDown.Padding = new Forms.Padding(4, 5, 4, 5);
        }

        private void LaunchGameFromTray(string exeName)
        {
            var target = _allGames.FirstOrDefault(g => g.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase))
                         ?? new GameItem { ExeName = exeName, Title = exeName.Replace(".exe", "", StringComparison.OrdinalIgnoreCase) };

            SelectGame(target);
            BtnStart_Click(this, new RoutedEventArgs());
        }

        public void BringWindowToFront()
        {
            Dispatcher.Invoke(() =>
            {
                Show();
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            });
        }

        private void DisposeSystemTray()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }
            catch { }
        }
        #endregion

        private void WebBadge_MouseDown(object sender, MouseButtonEventArgs e) => OpenUrl("https://haytool.online/FakeLord/");
        private void LinkWebsite_Click(object sender, RoutedEventArgs e) => OpenUrl("https://haytool.online/FakeLord/");
        private void LinkGithub_Click(object sender, RoutedEventArgs e) => OpenUrl("https://github.com/HaYToKoRaZ/FakeLord");
        private void LinkIssues_Click(object sender, RoutedEventArgs e) => OpenUrl("https://github.com/HaYToKoRaZ/FakeLord/issues");
        private void LinkEmail_Click(object sender, RoutedEventArgs e) => OpenUrl("mailto:korazhayto@gmail.com");

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch { }
        }
        #endregion

        #region Bildirimler (Toast) & Sürüm Kontrolü
        private Action? _toastAction = null;
        private DispatcherTimer? _toastTimer = null;
        private string? _latestReleaseUrl = null;

        private void ShowToast(string message, string icon = "ℹ️", Action? onClick = null)
        {
            Dispatcher.Invoke(() =>
            {
                TxtToastIcon.Text = icon;
                TxtToastMessage.Text = message;
                _toastAction = onClick;
                TxtToastAction.Visibility = onClick != null ? Visibility.Visible : Visibility.Collapsed;

                ToastBorder.Visibility = Visibility.Visible;
                ToastBorder.Opacity = 1;

                if (_toastTimer == null)
                {
                    _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
                    _toastTimer.Tick += (s, e) =>
                    {
                        _toastTimer.Stop();
                        ToastBorder.Visibility = Visibility.Collapsed;
                    };
                }
                else
                {
                    _toastTimer.Stop();
                }
                _toastTimer.Start();
            });
        }

        private void ToastBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _toastTimer?.Stop();
            ToastBorder.Visibility = Visibility.Collapsed;
            _toastAction?.Invoke();
        }

        private void VersionBadge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string url = !string.IsNullOrEmpty(_latestReleaseUrl) ? _latestReleaseUrl : "https://github.com/HaYToKoRaZ/FakeLord/releases";
            OpenUrl(url);
        }

        private async Task CheckForGitHubUpdatesAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("FakeLord-App");
                client.Timeout = TimeSpan.FromSeconds(4);

                var response = await client.GetStringAsync("https://api.github.com/repos/HaYToKoRaZ/FakeLord/releases/latest");
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                string tagName = root.TryGetProperty("tag_name", out var tagElem) ? (tagElem.GetString() ?? "") : "";
                string htmlUrl = root.TryGetProperty("html_url", out var urlElem) ? (urlElem.GetString() ?? "") : "https://github.com/HaYToKoRaZ/FakeLord/releases";

                if (string.IsNullOrWhiteSpace(tagName)) return;

                string cleanRemote = tagName.Trim().TrimStart('v', 'V');
                Version currentVer = new Version(1, 4, 0);

                if (Version.TryParse(cleanRemote, out var remoteVer) || 
                    Version.TryParse(cleanRemote + ".0", out remoteVer))
                {
                    if (remoteVer > currentVer)
                    {
                        _latestReleaseUrl = htmlUrl;
                        Dispatcher.Invoke(() =>
                        {
                            NewVersionDot.Visibility = Visibility.Visible;
                            VersionBadgeBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                            VersionBadgeBorder.ToolTip = LocalizationManager.Get("TooltipNewVersion", tagName);

                            string msg = LocalizationManager.Get("ToastNewVersion", tagName);
                            ShowToast(msg, "🚀", () => OpenUrl(_latestReleaseUrl));
                        });
                    }
                }
            }
            catch
            {
                // Sessizce yut: Çevrimdışı veya henüz release yoksa kullanıcı akışını bölme
            }
        }
        #endregion
    }
}