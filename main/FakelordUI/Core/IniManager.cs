using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FakelordUI.Core
{
    public static class IniManager
    {
        private static string IniPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

        public static string LastGame { get; set; } = "cs2.exe";
        public static string CustomGameImage { get; set; } = "";
        public static bool AutoFetchDiscord { get; set; } = true;
        public static bool StartMinimized { get; set; } = false;
        public static string Language { get; set; } = "TR"; // TR or EN
        public static string Theme { get; set; } = "ObsidianAbyss"; // ObsidianAbyss, DiscordNitro, VaporwaveSunset, AbyssalOcean, RogueCrimson, MatrixEmerald, SolarFlare, AmethystNight
        public static List<string> Favorites { get; set; } = new() { "cs2.exe", "GTA5.exe", "win64/valorant-win64-shipping.exe", "league of legends.exe" };

        // Yeni Eklenen Ayarlar
        public static bool EnableSystemTray { get; set; } = true;
        public static bool MinimizeToTray { get; set; } = true;
        public static bool StartWithWindows { get; set; } = false;
        public static double WindowWidth { get; set; } = 740;
        public static double WindowHeight { get; set; } = 670;
        public static double WindowLeft { get; set; } = -1; // -1: varsayilan merkez
        public static double WindowTop { get; set; } = -1;
        public static List<string> RecentGames { get; set; } = new();

        public static void AddRecentGame(string exeName)
        {
            if (string.IsNullOrWhiteSpace(exeName)) return;

            RecentGames.RemoveAll(x => x.Equals(exeName, StringComparison.OrdinalIgnoreCase));
            RecentGames.Insert(0, exeName);

            if (RecentGames.Count > 10)
            {
                RecentGames = RecentGames.Take(10).ToList();
            }

            Save();
        }

        public static void Load()
        {
            if (!File.Exists(IniPath))
            {
                Save();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(IniPath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";")) continue;

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    var val = parts[1].Trim();

                    switch (key.ToLower())
                    {
                        case "lastgame":
                            LastGame = val;
                            break;
                        case "customgameimage":
                        case "customimage":
                            CustomGameImage = val;
                            break;
                        case "autofetchdiscord":
                            AutoFetchDiscord = bool.TryParse(val, out var b1) ? b1 : true;
                            break;
                        case "startminimized":
                            StartMinimized = bool.TryParse(val, out var b2) ? b2 : false;
                            break;
                        case "enablesystemtray":
                        case "systemtray":
                            EnableSystemTray = bool.TryParse(val, out var bTray) ? bTray : true;
                            break;
                        case "minimizetotray":
                            MinimizeToTray = bool.TryParse(val, out var bMinTray) ? bMinTray : true;
                            break;
                        case "startwithwindows":
                        case "autostart":
                            StartWithWindows = bool.TryParse(val, out var bAuto) ? bAuto : false;
                            break;
                        case "windowwidth":
                            if (double.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, out var w) && w >= 400)
                                WindowWidth = w;
                            break;
                        case "windowheight":
                            if (double.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, out var h) && h >= 400)
                                WindowHeight = h;
                            break;
                        case "windowleft":
                            if (double.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, out var left))
                                WindowLeft = left;
                            break;
                        case "windowtop":
                            if (double.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, out var top))
                                WindowTop = top;
                            break;
                        case "recentgames":
                            RecentGames = val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                             .Distinct(StringComparer.OrdinalIgnoreCase)
                                             .Take(10)
                                             .ToList();
                            break;
                        case "language":
                            Language = val.ToUpper() == "EN" ? "EN" : "TR";
                            break;
                        case "theme":
                            if (val.Equals("CyberDark", StringComparison.OrdinalIgnoreCase)) Theme = "ObsidianAbyss";
                            else if (val.Equals("DiscordBlurple", StringComparison.OrdinalIgnoreCase)) Theme = "DiscordNitro";
                            else if (val.Equals("MidnightBlue", StringComparison.OrdinalIgnoreCase)) Theme = "AbyssalOcean";
                            else Theme = val;
                            break;
                        case "favorites":
                            Favorites = val.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Distinct(StringComparer.OrdinalIgnoreCase)
                                           .ToList();
                            break;
                    }
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("# ============================================================================");
                sb.AppendLine("# FAKELORD YAPILANDIRMA DOSYASI / CONFIGURATION FILE");
                sb.AppendLine("# Otomatik olusturulmustur. Degerleri buradan veya uygulama icinden degistirebilirsiniz.");
                sb.AppendLine("# Automatically generated. You can modify values here or via the in-app settings.");
                sb.AppendLine("# ============================================================================");
                sb.AppendLine();
                sb.AppendLine("[Ayarlar]");
                sb.AppendLine();

                sb.AppendLine("# [TR] En son secilen veya baslatilan oyunun calistirilabilir dosya adi (.exe)");
                sb.AppendLine("# [EN] The executable (.exe) name of the last selected or launched game");
                sb.AppendLine($"LastGame = {LastGame}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Ozel oyun simgesi dosya yolu veya URL'si (bos birakilabilir)");
                sb.AppendLine("# [EN] Custom game icon file path or URL (can be left empty)");
                sb.AppendLine($"CustomGameImage = {CustomGameImage}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Arayuz dili / [EN] Interface language (TR / EN)");
                sb.AppendLine($"Language = {Language}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Secili tema / [EN] Active color theme");
                sb.AppendLine("# (ObsidianAbyss, DiscordNitro, VaporwaveSunset, AbyssalOcean, RogueCrimson, MatrixEmerald, SolarFlare, AmethystNight)");
                sb.AppendLine($"Theme = {Theme}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Favori oyunlar listesi (virgulle ayrilmis exe adlari)");
                sb.AppendLine("# [EN] Favorite games list (comma-separated exe names)");
                sb.AppendLine($"Favorites = {string.Join(",", Favorites)}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Son acilan oyunlar gecmisi (Sistem tepsisi hizli baslatma icin, en son 10 adet)");
                sb.AppendLine("# [EN] Recent games history (For system tray quick launch, up to 10)");
                sb.AppendLine($"RecentGames = {string.Join(",", RecentGames)}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Acilista Discord oyun katalogunu otomatik guncelle (True / False)");
                sb.AppendLine("# [EN] Automatically update Discord games catalog on startup (True / False)");
                sb.AppendLine($"AutoFetchDiscord = {AutoFetchDiscord}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Sistem tepsisi (System Tray) simgesini aktif et (True / False)");
                sb.AppendLine("# [EN] Enable system tray notification icon (True / False)");
                sb.AppendLine($"EnableSystemTray = {EnableSystemTray}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Kapatildiginda veya simge durumuna getirildiginde tepsiye kucult (True / False)");
                sb.AppendLine("# [EN] Minimize to system tray when minimized or closed (True / False)");
                sb.AppendLine($"MinimizeToTray = {MinimizeToTray}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Windows acilisinda otomatik baslat (True / False)");
                sb.AppendLine("# [EN] Start automatically with Windows (True / False)");
                sb.AppendLine($"StartWithWindows = {StartWithWindows}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Uygulama basladiginda kucultulmus baslasin (True / False)");
                sb.AppendLine("# [EN] Start minimized to tray on launch (True / False)");
                sb.AppendLine($"StartMinimized = {StartMinimized}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Son pencere genisligi ve yuksekligi");
                sb.AppendLine("# [EN] Saved window width and height");
                sb.AppendLine($"WindowWidth = {WindowWidth.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                sb.AppendLine($"WindowHeight = {WindowHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                sb.AppendLine();

                sb.AppendLine("# [TR] Son pencere ekran koordinatlari (-1 ise varsayilan ekran ortasi)");
                sb.AppendLine("# [EN] Saved window screen position (-1 for default center)");
                sb.AppendLine($"WindowLeft = {WindowLeft.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                sb.AppendLine($"WindowTop = {WindowTop.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

                File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }
    }
}
