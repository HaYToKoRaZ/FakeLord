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
        public static string Theme { get; set; } = "ObsidianAbyss"; // ObsidianAbyss, DiscordNitro, VaporwaveSunset, AbyssalOcean, RogueCrimson, MatrixEmerald
        public static List<string> Favorites { get; set; } = new() { "cs2.exe", "GTA5.exe", "win64/valorant-win64-shipping.exe", "league of legends.exe" };

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
                sb.AppendLine("# Fakelord Yapilandirma Dosyasi");
                sb.AppendLine("[Ayarlar]");
                sb.AppendLine($"LastGame = {LastGame}");
                sb.AppendLine($"CustomGameImage = {CustomGameImage}");
                sb.AppendLine($"Language = {Language}");
                sb.AppendLine($"Theme = {Theme}");
                sb.AppendLine($"Favorites = {string.Join(",", Favorites)}");
                sb.AppendLine($"AutoFetchDiscord = {AutoFetchDiscord}");
                sb.AppendLine($"StartMinimized = {StartMinimized}");

                File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }
    }
}
