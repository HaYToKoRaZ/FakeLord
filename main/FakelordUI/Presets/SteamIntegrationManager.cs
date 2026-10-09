using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace FakelordUI.Presets
{
    public static class SteamIntegrationManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static SteamIntegrationManager()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        #region 1. Steam Top 100 En Çok Oynanan Oyunları Çekme
        public static async Task<List<GameItem>> FetchSteamTop100Async(List<GameItem> existingCatalog)
        {
            var result = new List<GameItem>();
            try
            {
                string json = await _httpClient.GetStringAsync("https://steamspy.com/api.php?request=top100in2weeks");
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Discord detectable kataloğu ile hızlı exe eşleştirmesi
                var exeByAppId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var exeByTitle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var g in existingCatalog)
                {
                    if (!string.IsNullOrEmpty(g.SteamAppId) && !exeByAppId.ContainsKey(g.SteamAppId))
                        exeByAppId[g.SteamAppId] = g.ExeName;

                    if (!string.IsNullOrEmpty(g.Title) && !exeByTitle.ContainsKey(g.Title))
                        exeByTitle[g.Title] = g.ExeName;
                }

                foreach (var prop in root.EnumerateObject())
                {
                    var gameObj = prop.Value;
                    string appid = gameObj.TryGetProperty("appid", out var a) ? a.ToString() : prop.Name;
                    string name = gameObj.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";

                    if (string.IsNullOrWhiteSpace(name)) continue;

                    string exe = "";
                    if (exeByAppId.TryGetValue(appid, out var matchedExe))
                        exe = matchedExe;
                    else if (exeByTitle.TryGetValue(name, out var matchedByTitle))
                        exe = matchedByTitle;
                    else
                    {
                        // Exe eşleşmesi henüz yoksa akıllı tahmin oluştur (örn: SonsOfTheForest.exe)
                        string clean = Regex.Replace(name, @"[^\w]", "");
                        exe = $"{clean}.exe";
                    }

                    result.Add(new GameItem
                    {
                        Id = appid,
                        Title = name,
                        ExeName = exe,
                        SteamAppId = appid,
                        CustomImageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appid}/header.jpg"
                    });
                }
            }
            catch { }

            return result;
        }
        #endregion

        #region 2. Bilgisayarda Yüklü Steam Oyunlarını Bulma (Registry & libraryfolders.vdf)
        public static List<GameItem> DetectInstalledSteamGames(List<GameItem> existingCatalog)
        {
            var installedGames = new List<GameItem>();
            var steamPath = GetSteamInstallPath();

            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
                return installedGames;

            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamPath };

            // libraryfolders.vdf dosyasından tüm kütüphane disklerini bul
            string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdfPath))
            {
                try
                {
                    string vdfContent = File.ReadAllText(vdfPath);
                    var matches = Regex.Matches(vdfContent, @"""path""\s+""([^""]+)""");
                    foreach (Match m in matches)
                    {
                        string p = m.Groups[1].Value.Replace(@"\\", @"\");
                        if (Directory.Exists(p)) libraryPaths.Add(p);
                    }
                }
                catch { }
            }

            // Her kütüphanedeki steamapps klasöründen manifest'leri tara
            foreach (var lib in libraryPaths)
            {
                string appsDir = Path.Combine(lib, "steamapps");
                if (!Directory.Exists(appsDir)) continue;

                var manifestFiles = Directory.GetFiles(appsDir, "appmanifest_*.acf");
                foreach (var mf in manifestFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(mf);
                        string appId = Regex.Match(content, @"""appid""\s+""(\d+)""").Groups[1].Value;
                        string gameName = Regex.Match(content, @"""name""\s+""([^""]+)""").Groups[1].Value;
                        string installDir = Regex.Match(content, @"""installdir""\s+""([^""]+)""").Groups[1].Value;

                        // Steamworks Common Redistributables veya proton gibi yardımcıları atla
                        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(gameName) || appId == "228980") continue;

                        string commonDir = Path.Combine(appsDir, "common", installDir);
                        string foundExe = "";

                        // Önce mevcut Discord kataloğundan doğrulanmış exe eşleşmesi var mı bak
                        var catMatch = existingCatalog.FirstOrDefault(c => c.SteamAppId == appId || c.Title.Equals(gameName, StringComparison.OrdinalIgnoreCase));
                        if (catMatch != null && !string.IsNullOrEmpty(catMatch.ExeName))
                        {
                            foundExe = catMatch.ExeName;
                        }
                        else if (Directory.Exists(commonDir))
                        {
                            var options = new EnumerationOptions
                            {
                                IgnoreInaccessible = true,
                                RecurseSubdirectories = true,
                                MaxRecursionDepth = 3
                            };
                            var exes = Directory.GetFiles(commonDir, "*.exe", options)
                                                .Where(e => !Path.GetFileName(e).StartsWith("unins", StringComparison.OrdinalIgnoreCase)
                                                         && !Path.GetFileName(e).Contains("crash", StringComparison.OrdinalIgnoreCase)
                                                         && !Path.GetFileName(e).Contains("report", StringComparison.OrdinalIgnoreCase)
                                                         && !Path.GetFileName(e).Contains("helper", StringComparison.OrdinalIgnoreCase)
                                                         && !Path.GetFileName(e).Contains("vcredist", StringComparison.OrdinalIgnoreCase)
                                                         && !Path.GetFileName(e).Contains("directx", StringComparison.OrdinalIgnoreCase))
                                                .ToList();

                            if (exes.Count > 0)
                            {
                                // İsim benzerliğine göre öncelik ver
                                var bestMatch = exes.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e).Equals(installDir, StringComparison.OrdinalIgnoreCase))
                                             ?? exes.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e).Contains(installDir, StringComparison.OrdinalIgnoreCase))
                                             ?? exes[0];

                                foundExe = Path.GetFileName(bestMatch);
                            }
                        }

                        if (string.IsNullOrEmpty(foundExe))
                        {
                            foundExe = $"{Regex.Replace(gameName, @"[^\w]", "")}.exe";
                        }

                        installedGames.Add(new GameItem
                        {
                            Id = appId,
                            Title = gameName,
                            ExeName = foundExe,
                            SteamAppId = appId,
                            CustomImageUrl = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg"
                        });
                    }
                    catch { }
                }
            }

            return installedGames;
        }

        private static string GetSteamInstallPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key != null)
                {
                    string? path = key.GetValue("SteamPath") as string;
                    if (!string.IsNullOrEmpty(path)) return path.Replace('/', '\\');
                }
            }
            catch { }

            string defaultPath = @"C:\Program Files (x86)\Steam";
            if (Directory.Exists(defaultPath)) return defaultPath;

            return @"D:\Steam";
        }
        #endregion

        #region 3. Steam AppID veya Store Linki ile Özel Oyun Bilgisi Çekme
        public static async Task<GameItem?> FetchGameByAppIdOrUrlAsync(string input, List<GameItem> catalog)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            string appId = input.Trim();
            // Eğer URL girilmişse linkten appid ayıkla (örn: store.steampowered.com/app/730/...)
            var match = Regex.Match(input, @"app/(\d+)");
            if (match.Success)
            {
                appId = match.Groups[1].Value;
            }

            if (!int.TryParse(appId, out _)) return null;

            try
            {
                string json = await _httpClient.GetStringAsync($"https://store.steampowered.com/api/appdetails?appids={appId}");
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty(appId, out var appElement))
                {
                    bool success = appElement.TryGetProperty("success", out var s) && s.GetBoolean();
                    if (success && appElement.TryGetProperty("data", out var data))
                    {
                        string name = data.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                        string headerImg = data.TryGetProperty("header_image", out var hi) ? hi.GetString() ?? "" : "";

                        // Exe eşleşmesi kontrol et
                        string exe = "";
                        var catMatch = catalog.FirstOrDefault(c => c.SteamAppId == appId || c.Title.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (catMatch != null) exe = catMatch.ExeName;
                        else exe = $"{Regex.Replace(name, @"[^\w]", "")}.exe";

                        return new GameItem
                        {
                            Id = appId,
                            Title = name,
                            ExeName = exe,
                            SteamAppId = appId,
                            CustomImageUrl = !string.IsNullOrEmpty(headerImg) ? headerImg : $"https://cdn.cloudflare.steamstatic.com/steam/apps/{appId}/header.jpg"
                        };
                    }
                }
            }
            catch { }

            return null;
        }
        #endregion
    }
}
