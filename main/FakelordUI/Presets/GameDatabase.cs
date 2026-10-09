using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using FakelordUI.Core;

namespace FakelordUI.Presets
{
    public class GameItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string ExeName { get; set; } = "";
        public string DisplayExeName => Path.GetFileName(ExeName);
        public string IconHash { get; set; } = "";
        public string SteamAppId { get; set; } = "";
        public string CustomImageUrl { get; set; } = "";

        public Visibility SteamBadgeVisibility => string.IsNullOrEmpty(SteamAppId) ? Visibility.Collapsed : Visibility.Visible;

        // 1. Özel Url -> 2. Steam Header -> 3. Discord CDN -> 4. Ini Özel Görsel Fallback
        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomImageUrl))
                    return CustomImageUrl;

                if (!string.IsNullOrEmpty(SteamAppId))
                    return $"https://cdn.cloudflare.steamstatic.com/steam/apps/{SteamAppId}/header.jpg";

                if (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(IconHash))
                    return $"https://cdn.discordapp.com/app-icons/{Id}/{IconHash}.png?size=256";

                if (!string.IsNullOrEmpty(IniManager.CustomGameImage))
                    return IniManager.CustomGameImage;

                return "";
            }
        }
    }

    public static class GameDatabase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string CacheFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "discord_detectable_cache.json");

        // Bilinen popüler oyunların Steam App ID veya doğrudan görsel bağlantıları
        private static readonly Dictionary<string, (string SteamId, string CustomUrl)> WellKnownGames = new(StringComparer.OrdinalIgnoreCase)
        {
            { "cs2.exe", ("730", "") },
            { "csgo.exe", ("730", "") },
            { "gta5.exe", ("271590", "") },
            { "gta_sa.exe", ("12120", "") },
            { "cyberpunk2077.exe", ("1091500", "") },
            { "rdr2.exe", ("1174180", "") },
            { "eldenring.exe", ("1245620", "") },
            { "hl2.exe", ("220", "") },
            { "rust.exe", ("252490", "") },
            { "rustclient.exe", ("252490", "") },
            { "dota2.exe", ("570", "") },
            { "pubg.exe", ("578080", "") },
            { "tslgame.exe", ("578080", "") },
            { "rainbowsix.exe", ("359550", "") },
            { "rainbowsixgame.exe", ("359550", "") },
            { "witcher3.exe", ("292030", "") },
            { "rocketleague.exe", ("252950", "") },
            { "apex.exe", ("1172470", "") },
            { "r5apex.exe", ("1172470", "") },
            { "eurotrucks2.exe", ("227300", "") },
            { "baldursgate3.exe", ("1086940", "") },
            { "bg3.exe", ("1086940", "") },
            { "bg3_dx11.exe", ("1086940", "") },
            { "left4dead2.exe", ("550", "") },
            { "arma3.exe", ("107410", "") },
            { "payday2_win32_release.exe", ("218620", "") },
            { "terraria.exe", ("105600", "") },
            { "fallout4.exe", ("377160", "") },
            { "skyrim.exe", ("72850", "") },
            { "valorant-win64-shipping.exe", ("", "https://images.contentstack.io/v3/assets/blt0eb2a2986bbfbe7a/blt7204646ec5607590/6447fd7d70cfd076d333f269/VAL_Ep7_A3_CG_horizontal_textless_1920x1080.jpg") },
            { "win64/valorant-win64-shipping.exe", ("", "https://images.contentstack.io/v3/assets/blt0eb2a2986bbfbe7a/blt7204646ec5607590/6447fd7d70cfd076d333f269/VAL_Ep7_A3_CG_horizontal_textless_1920x1080.jpg") },
            { "league of legends.exe", ("", "https://cmsassets.rgpub.io/sanity/images/dsfx7636/news/c31562ae0eb49a0d81065ea07e6022e37e9fb35d-1920x1080.jpg") },
            { "leagueclientux.exe", ("", "https://cmsassets.rgpub.io/sanity/images/dsfx7636/news/c31562ae0eb49a0d81065ea07e6022e37e9fb35d-1920x1080.jpg") },
            { "minecraft.exe", ("", "https://www.minecraft.net/content/dam/games/minecraft/key-art/Minecraft-hero-image-1280x720.jpg") },
            { "javaw.exe", ("", "https://www.minecraft.net/content/dam/games/minecraft/key-art/Minecraft-hero-image-1280x720.jpg") },
            { "fortniteclient-win64-shipping.exe", ("", "https://cdn2.unrealengine.com/social-image-chapter4-s3-3840x2160-d35919020e96.jpg") }
        };

        public static List<GameItem> LoadCachedGames()
        {
            if (File.Exists(CacheFile))
            {
                try
                {
                    string json = File.ReadAllText(CacheFile);
                    var cached = ParseDiscordDetectable(json);
                    if (cached.Count > 0) return cached;
                }
                catch { }
            }

            return GetDefaultPopularGames();
        }

        public static async Task<List<GameItem>> FetchDiscordDetectableAsync()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                string json = await _httpClient.GetStringAsync("https://discord.com/api/v9/applications/detectable");
                await File.WriteAllTextAsync(CacheFile, json);
                return ParseDiscordDetectable(json);
            }
            catch (Exception)
            {
                return GetDefaultPopularGames();
            }
        }

        public static List<GameItem> ParseDiscordDetectable(string json)
        {
            var games = new List<GameItem>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var app in root.EnumerateArray())
                    {
                        string id = app.TryGetProperty("id", out var i) ? i.GetString() ?? "" : "";
                        string name = app.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                        string icon = app.TryGetProperty("icon", out var ic) ? ic.GetString() ?? "" : "";
                        if (string.IsNullOrEmpty(icon) && app.TryGetProperty("icon_hash", out var ich))
                        {
                            icon = ich.GetString() ?? "";
                        }

                        if (string.IsNullOrWhiteSpace(name) || seenNames.Contains(name)) continue;

                        string steamId = "";
                        if (app.TryGetProperty("third_party_skus", out var skus) && skus.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var sku in skus.EnumerateArray())
                            {
                                string dist = sku.TryGetProperty("distributor", out var d) ? d.GetString() ?? "" : "";
                                if (dist.Equals("steam", StringComparison.OrdinalIgnoreCase))
                                {
                                    steamId = sku.TryGetProperty("id", out var sid) ? sid.GetString() ?? "" : "";
                                    if (!string.IsNullOrEmpty(steamId)) break;
                                }
                            }
                        }

                        string exe = "";
                        if (app.TryGetProperty("executables", out var exes))
                        {
                            exe = SelectBestExecutable(exes);
                        }

                        if (!string.IsNullOrEmpty(exe))
                        {
                            seenNames.Add(name);

                            var item = new GameItem
                            {
                                Id = id,
                                Title = name,
                                ExeName = exe,
                                IconHash = icon,
                                SteamAppId = steamId
                            };

                            if (WellKnownGames.TryGetValue(exe, out var info))
                            {
                                if (!string.IsNullOrEmpty(info.SteamId)) item.SteamAppId = info.SteamId;
                                if (!string.IsNullOrEmpty(info.CustomUrl)) item.CustomImageUrl = info.CustomUrl;
                            }

                            games.Add(item);
                        }
                    }
                }
            }
            catch { }

            // Popüler varsayılan oyunları en üste veya listeye ekle
            var defaults = GetDefaultPopularGames();
            foreach (var d in defaults)
            {
                if (!games.Any(g => g.ExeName.Equals(d.ExeName, StringComparison.OrdinalIgnoreCase)))
                {
                    games.Insert(0, d);
                }
            }

            return games.Count > 0 ? games : defaults;
        }

        public static List<GameItem> GetDefaultPopularGames()
        {
            return new List<GameItem>
            {
                new GameItem 
                { 
                    Id = "700136079562375258", 
                    Title = "VALORANT", 
                    ExeName = "win64/valorant-win64-shipping.exe", 
                    IconHash = "11f81959f4fdd76ca6c39c59eac256c1",
                    CustomImageUrl = "https://images.contentstack.io/v3/assets/blt0eb2a2986bbfbe7a/blt7204646ec5607590/6447fd7d70cfd076d333f269/VAL_Ep7_A3_CG_horizontal_textless_1920x1080.jpg"
                },
                new GameItem 
                { 
                    Id = "1402418696126992445", 
                    Title = "League of Legends", 
                    ExeName = "league of legends.exe", 
                    IconHash = "7c99428541032ac02ec6981d88b78fb7",
                    CustomImageUrl = "https://cmsassets.rgpub.io/sanity/images/dsfx7636/news/c31562ae0eb49a0d81065ea07e6022e37e9fb35d-1920x1080.jpg"
                },
                new GameItem 
                { 
                    Id = "791942006378823700", 
                    Title = "Counter-Strike 2", 
                    ExeName = "cs2.exe", 
                    SteamAppId = "730", 
                    IconHash = "d9fa0e882a1705e3a35b1c97a8ecdc93" 
                },
                new GameItem 
                { 
                    Id = "406939665672536064", 
                    Title = "Grand Theft Auto V", 
                    ExeName = "GTA5.exe", 
                    SteamAppId = "271590", 
                    IconHash = "85888a75e3c7901768c62b48b1bfb990" 
                },
                new GameItem 
                { 
                    Id = "432980957394370572", 
                    Title = "Fortnite", 
                    ExeName = "fortniteclient-win64-shipping.exe", 
                    CustomImageUrl = "https://cdn2.unrealengine.com/social-image-chapter4-s3-3840x2160-d35919020e96.jpg" 
                },
                new GameItem 
                { 
                    Id = "435443705055346690", 
                    Title = "Minecraft", 
                    ExeName = "minecraft.exe", 
                    CustomImageUrl = "https://www.minecraft.net/content/dam/games/minecraft/key-art/Minecraft-hero-image-1280x720.jpg" 
                },
                new GameItem 
                { 
                    Id = "541738403230777351", 
                    Title = "Apex Legends", 
                    ExeName = "r5apex.exe", 
                    SteamAppId = "1172470" 
                },
                new GameItem 
                { 
                    Id = "356875221078245376", 
                    Title = "Overwatch 2", 
                    ExeName = "overwatch.exe", 
                    SteamAppId = "2357570",
                    IconHash = "a60bb76ba4d4acafbd4cb9aad6e61739" 
                },
                new GameItem 
                { 
                    Id = "367827983903490050", 
                    Title = "PUBG: BATTLEGROUNDS", 
                    ExeName = "tslgame.exe", 
                    SteamAppId = "578080" 
                },
                new GameItem 
                { 
                    Id = "356877880938070016", 
                    Title = "Rocket League", 
                    ExeName = "rocketleague.exe", 
                    SteamAppId = "252950",
                    IconHash = "a74899a5190c48a3e6ce9f8d2eaff348" 
                },
                new GameItem 
                { 
                    Id = "363409867568021505", 
                    Title = "Rust", 
                    ExeName = "rustclient.exe", 
                    SteamAppId = "252490" 
                },
                new GameItem 
                { 
                    Id = "533413476366811136", 
                    Title = "Red Dead Redemption 2", 
                    ExeName = "RDR2.exe", 
                    SteamAppId = "1174180", 
                    IconHash = "3ca6a4b11f75323cb1d4f64700d6194b" 
                },
                new GameItem 
                { 
                    Id = "785888320498728980", 
                    Title = "Cyberpunk 2077", 
                    ExeName = "Cyberpunk2077.exe", 
                    SteamAppId = "1091500", 
                    IconHash = "cdde4ee3bca413349929f9573887c331" 
                },
                new GameItem 
                { 
                    Id = "356876590342340608", 
                    Title = "Tom Clancy's Rainbow Six Siege", 
                    ExeName = "rainbowsix.exe", 
                    SteamAppId = "359550", 
                    IconHash = "01125e693db476e6f83f7d9769080fd0" 
                },
                new GameItem 
                { 
                    Id = "356875988589740042", 
                    Title = "Dota 2", 
                    ExeName = "dota2.exe", 
                    SteamAppId = "570", 
                    IconHash = "6b4b3fa4c83555d3008de69d33a60588" 
                },
                new GameItem 
                { 
                    Id = "889506696146681907", 
                    Title = "Elden Ring", 
                    ExeName = "eldenring.exe", 
                    SteamAppId = "1245620", 
                    IconHash = "4bbf319e71ec269894e6fe8423f81e3a" 
                },
                new GameItem 
                { 
                    Id = "754759600124887050", 
                    Title = "Genshin Impact", 
                    ExeName = "genshinimpact.exe", 
                    CustomImageUrl = "https://images2.alphacoders.com/110/1109233.jpg" 
                },
                new GameItem 
                { 
                    Id = "356875762940379136", 
                    Title = "World of Warcraft", 
                    ExeName = "_retail_/wow.exe", 
                    IconHash = "fc92f820c44e72085dc6205e5e746850" 
                }
            };
        }

        public static string SelectBestExecutable(JsonElement exesElement)
        {
            if (exesElement.ValueKind != JsonValueKind.Array) return "";

            string bestExe = "";
            int bestScore = int.MinValue;

            foreach (var e in exesElement.EnumerateArray())
            {
                string os = e.TryGetProperty("os", out var o) ? o.GetString() ?? "" : "";
                if (!os.Equals("win32", StringComparison.OrdinalIgnoreCase)) continue;

                string exeName = e.TryGetProperty("name", out var en) ? en.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(exeName)) continue;

                bool isLauncher = e.TryGetProperty("is_launcher", out var isl) && isl.GetBoolean();

                int score = 0;

                // 1. is_launcher kontrolü (Oyun süreçleri launcher değildir)
                if (!isLauncher) score += 50;
                else score -= 30;

                string lower = exeName.ToLowerInvariant();
                string fileName = Path.GetFileName(lower);

                // 2. Yardımcı/çökme raporlayıcı süreçleri filtrele
                if (fileName.Contains("crash") || fileName.Contains("report") || fileName.Contains("unins") || 
                    fileName.Contains("update") || fileName.Contains("eac") || fileName.Contains("battleye") ||
                    fileName.Contains("easyanticheat") || fileName.Contains("anticheat"))
                {
                    score -= 100;
                }

                // 3. Jenerik isim & Yol analizi (game.exe, client.exe, launcher.exe)
                bool isGeneric = fileName is "game.exe" or "client.exe" or "launcher.exe" or "main.exe" or "start.exe";
                bool hasPath = exeName.Contains('/') || exeName.Contains('\\');

                if (hasPath)
                {
                    // Discord yol içeren bir exe tanımlamışsa (Örn: win64/valorant-win64-shipping.exe veya command & conquer red alert ii/game.exe)
                    // Bu kural Discord'un çakışmayı önlemek için aradığı resmi yoldur!
                    score += 40;
                }
                else if (isGeneric)
                {
                    // Yolsuz jenerik exe (tek başına "game.exe"), Discord tarafından çakışma nedeniyle tanınmaz
                    score -= 20;
                }

                // 4. Win64 / Shipping / Release ana binary tercihleri
                if (lower.Contains("win64") || lower.Contains("x64") || lower.Contains("shipping"))
                {
                    score += 25;
                }

                // 5. "launcher.exe" veya "start.exe" tek başına ise düşük puan
                if (fileName is "launcher.exe" or "start.exe" or "autorun.exe")
                {
                    score -= 40;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestExe = exeName;
                }
            }

            return bestExe;
        }
    }
}
