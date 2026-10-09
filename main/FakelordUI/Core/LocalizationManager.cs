using System;
using System.Collections.Generic;

namespace FakelordUI.Core
{
    public static class LocalizationManager
    {
        public static string CurrentLanguage => IniManager.Language;

        private static readonly Dictionary<string, (string TR, string EN)> Translations = new()
        {
            { "AppSubtitle", ("Discord Akıllı Oyun Algılama & Profil Simülatörü", "Discord Smart Game Detection & Profile Simulator") },
            { "UpdateCatalog", ("🔄 Kataloğu Güncelle", "🔄 Update Catalog") },
            { "UpdatingCatalog", ("⏳ İndiriliyor...", "⏳ Downloading...") },
            { "SearchPlaceholder", ("Oyun ara veya Steam AppID gir (Örn: 730, cs2, GTA)...", "Search game or enter Steam AppID (e.g. 730, cs2, GTA)...") },
            { "LiveCardTitle", ("Discord Profil Önizlemesi", "Discord Profile Preview") },
            { "PlayingAGame", ("Şu Anda Oynanıyor", "Currently Playing") },
            { "NoGameSelected", ("Oyun Seçilmedi", "No Game Selected") },
            { "WaitingExe", ("Exe: Bekleniyor...", "Exe: Waiting...") },
            { "ReadyToPlay", ("Oynamaya hazır", "Ready to play") },
            { "PlayingStatus", ("Şu anda oynuyor • Discord Profilinizde Aktif", "Currently playing • Active on Discord Profile") },
            { "StoppedStatus", ("Durduruldu", "Stopped") },
            { "PlayOnDiscord", ("▶ Discord'da Oyna", "▶ Play on Discord") },
            { "StopGame", ("⏹ Oyunu Durdur", "⏹ Stop Game") },
            { "ReadyStatusMsg", ("● Hazır. Listeden veya Steam sekmelerinden bir oyun seçin.", "● Ready. Select a game from the list or Steam tabs.") },
            { "ActiveStatusMsg", ("● Aktif: '{0}' Discord profilinizde gösteriliyor!", "● Active: '{0}' is now showing on your Discord profile!") },
            { "StoppedStatusMsg", ("● Oyun durduruldu. Discord durumu temizlendi.", "● Game stopped. Discord activity cleared.") },
            { "CatalogUpdatingMsg", ("● Discord resmi Detectable Games API'sinden 3.000+ oyun indiriliyor...", "● Fetching 3,000+ games from Discord Detectable API...") },
            { "CatalogSuccessMsg", ("● Başarılı! {0} oyun yüklendi.", "● Success! {0} games loaded.") },
            { "CatalogErrorMsg", ("❌ Discord listesi güncellenirken bir sorun oluştu.", "❌ Failed to update Discord game catalog.") },
            { "FavTitle", ("Favoriler:", "Favorites:") },
            { "AddFav", ("⭐ Favorilere Ekle", "⭐ Add to Favs") },
            { "RemoveFav", ("★ Favorilerden Çıkar", "★ Remove Fav") },
            { "Theme_ObsidianAbyss", ("🌑 Obsidyen Gece", "🌑 Obsidian Abyss") },
            { "Theme_DiscordNitro", ("🎮 Discord Nitro", "🎮 Discord Nitro") },
            { "Theme_VaporwaveSunset", ("🌆 Siber Alacakaranlık", "🌆 Vaporwave Sunset") },
            { "Theme_AbyssalOcean", ("🌌 Derin Okyanus", "🌌 Abyssal Ocean") },
            { "Theme_RogueCrimson", ("🎯 Hayalet Kızıl", "🎯 Rogue Crimson") },
            { "Theme_MatrixEmerald", ("🟢 Matris Zümrüt", "🟢 Matrix Emerald") },
            { "Theme_SolarFlare", ("⚡ Güneş Patlaması", "⚡ Solar Flare") },
            { "Theme_AmethystNight", ("🔮 Ametist Gecesi", "🔮 Amethyst Night") },
            { "TabPopular", ("⚡ Popüler & Rekabetçi", "⚡ Popular & Esports") },
            { "TabAll", ("🎮 Tüm Oyunlar", "🎮 All Games") },
            { "TabSteamTop", ("🔥 Steam Top 100", "🔥 Steam Top 100") },
            { "TabSteamInstalled", ("💻 Yüklü Steam Oyunları", "💻 Installed Steam Games") },
            { "BtnAddSteamApp", ("+ Steam ID Ekle", "+ Add Steam ID") },
            { "SteamAddPrompt", ("Eklemek istediğiniz Steam Oyununun App ID'sini veya Mağaza Linkini girin:", "Enter Steam App ID or Store Link to add:") },
            { "SteamAddSuccess", ("● '{0}' başarıyla Steam'den çekildi ve kataloğa eklendi!", "● '{0}' successfully fetched from Steam and added to catalog!") },
            { "SteamAddError", ("❌ Belirtilen AppID ile oyun bulunamadı.", "❌ No game found with the specified Steam AppID.") },
            { "AboutBtnTooltip", ("Hakkında & İletişim", "About & Contact") },
            { "AboutTitle", ("Hakkında", "About") },
            { "AboutDeveloper", ("Geliştirici: HaYTo", "Developer: HaYTo") },
            { "AboutDesc", ("Projeyi geliştirmeme katkıda bulunmak, güncellemelerden haberdar olmak veya geri bildirim bırakmak için bağlantıları kullanabilirsiniz:", "To contribute to the project, stay informed about updates, or leave feedback, you can use the links below:") },
            { "AboutTwitterTitle", ("X (Twitter) • @HaYTo", "X (Twitter) • @HaYTo") },
            { "AboutTwitterDesc", ("Güncellemeler & Duyurular", "Updates & Announcements") },
            { "AboutGithubTitle", ("GitHub • HaYToKoRaZ/FakeLord", "GitHub • HaYToKoRaZ/FakeLord") },
            { "AboutGithubDesc", ("Kaynak Kod & ⭐ Yıldız Ver", "Source Code & Star on GitHub") },
            { "AboutIssuesTitle", ("GitHub Issues • Hata & İstek", "GitHub Issues • Bug & Requests") },
            { "AboutIssuesDesc", ("Geri Bildirim & Hata Bildirimi", "Feedback & Bug Reports") },
            { "AboutEmailTitle", ("E-posta • korazhayto@gmail.com", "Email • korazhayto@gmail.com") },
            { "AboutEmailDesc", ("Öneri & Doğrudan İletişim", "Suggestions & Direct Inquiries") },
            { "AboutClose", ("Kapat", "Close") },
            { "ToastUpdatingAll", ("Tüm kaynaklar (Discord 3.000+ & Steam Top 100) güncelleniyor...", "Updating all catalogs (Discord 3,000+ & Steam Top 100)...") },
            { "ToastUpdatedAll", ("Tüm kaynaklar (Discord 3.000+ & Steam Top 100) başarıyla güncellendi!", "All catalogs (Discord 3,000+ & Steam Top 100) updated!") },
            { "ToastNewVersion", ("Yeni FakeLord sürümü mevcut! ({0}) İndirmek için tıklayın.", "New FakeLord version available! ({0}) Click to download.") },
            { "TooltipNewVersion", ("Yeni sürüm mevcut ({0})! GitHub'dan indirmek için tıklayın.", "New version available ({0})! Click to download from GitHub.") },
            { "ToastGameStarted", ("'{0}' Discord'da aktif edildi!", "'{0}' is now active on Discord!") },
        };

        public static string Get(string key, params object[] args)
        {
            if (Translations.TryGetValue(key, out var pair))
            {
                string text = CurrentLanguage == "EN" ? pair.EN : pair.TR;
                if (args.Length > 0)
                {
                    try { return string.Format(text, args); } catch { }
                }
                return text;
            }
            return key;
        }
    }
}
