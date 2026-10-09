# Changelog

Tüm önemli değişiklikler bu dosyada belgelenecektir.

## [v1.7.0] - 2026-10-09

### 🐱 Modern İki Dilli Web Vitrini & Maskot Çakıl
- **🐱 Sevimli Kedi Maskot Çakıl (Interactive Gamer Mascot):** Ziyaretçileri karşılayan, oyuncu kulaklığı takan, tıklamalara, hover'a ve tema değişimlerine dinamik tepkiler veren etkileşimli SVG/CSS maskot eklendi.
- **🌐 Çift Dilli URL Yönlendirmeli Gezinme (Bilingual Navigation):** Sayfa içi geçişlerde ve bağlantılarda dil parametresi (`?lang=tr` / `?lang=en`) otomatik korunur. Sayfa yenilenmeden anında Türkçe ve İngilizce arasında pürüzsüz geçiş sağlanır.
- **🎨 Theme Factory Web Entegrasyonu:** Masaüstü uygulamasındaki 12 özgün renk teması web vitrinine de taşındı. Ziyaretçiler web sitesi üzerinden doğrudan canlı tema değiştirebilir ve interaktif Discord Profil Kartı simülasyonunu test edebilir.
- **🚀 Sıfır Bağımlılık & 100/100 Lighthouse Performansı:** React veya karmaşık paketleyicilere ihtiyaç duymadan, saf Semantic HTML5, Vanilla CSS ve modern JavaScript ile en yüksek hız, kusursuz SEO (`SoftwareApplication` & `FAQPage` JSON-LD şemaları) ve GitHub Pages uyumluluğu sağlandı.

---

## [v1.6.0] - 2026-10-09

### 🎨 Theme Factory Entegrasyonu & Bütünleşik Dinamik Tasarım
- **🎭 Anthropic Theme Factory Küratörlü Paletleri:** `Midnight Galaxy` 🌌, `Tech Innovation` ⚡, `Ocean Depths` 🌊, `Sunset Boulevard` 🌆, `Forest Canopy` 🌲, `Golden Hour` 🌅, `Arctic Frost` ❄️, `Desert Rose` 🏜️, `Botanical Garden` 🌿, `Modern Minimalist` 🖤 ile birlikte klasik oyuncu temaları `Discord Nitro` 🎮 ve `Rogue Crimson` 🎯 sisteme dahil edildi.
- **✨ Tam Senkronize Dinamik Vurgular (Dynamic Accent Theming):** Tema değiştiğinde yalnızca arka plan değil; Başlat Butonu (`BtnStart`), Aktif Kategori Sekmeleri (`Tümü / Top 100 / Yüklü`), A-Z Harf Şeridi seçili harf rozeti, Seçili Oyun Kartı ışıltısı ve Sürüm Rozeti seçilen temanın özgün accent rengiyle anında baştan aşağı senkronize olur.
- **📁 HaYTooL İzolasyonu (Data Klasörü Disiplini):** `derlenmis/` kök dizini tertemiz hale getirildi. `HaYTooL.exe` artık doğrudan `derlenmis/Data/` ve `derlenmis/Data/Runners/` klasörlerinde konumlandırıldı.

---

## [v1.5.0] - 2026-10-09

### ✨ Yeni Özellikler & Geliştirmeler
- **🔤 A-Z Hızlı Harf Şeridi (Alfabetik Quick-Jump Bar):** Oyun listesinin üstüne yerleştirilen interaktif `#` ve `A-Z` harf şeridi ile istenilen harfe anında atlama desteği.
- **⚡ Akıllı Liste Kaydırma (ScrollIntoView):** Tıklanan harfle başlayan ilk oyuna pürüzsüz kaydırma ve otomatik seçim.
- **🎯 Dinamik Harf Doluluk ve Aktif Harf Göstergesi:** Aktif sekmede mevcut olan harfler parlak ve tıklanabilir gösterilirken, bulunmayan harfler hafif saydamlaştırılır. Seçili oyun değiştikçe aktif harf otomatik vurgulanır.
- **⌨️ Klavyeden Seriden Atlama Desteği:** Liste veya pencere üzerindeyken doğrudan klavyeden herhangi bir harfe (`A-Z`) veya rakama (`#`) basıldığında anında ilgili oyun grubuna zıplama özelliği.
- **✨ Escape ile Hızlı Temizleme:** Klavyeden `Esc` tuşuna basıldığında arama kutusunu anında temizleme veya açık modalları kapatma konforu.

---

## [v1.2.0] - 2026-10-09

### ✨ Yeni Özellikler & Geliştirmeler
- **🌐 Otomatik GitHub Release Sürüm Kontrolü:** Uygulama açılışında en son GitHub sürümünü kontrol eder. Yeni sürüm varsa üst bardaki sürüm rozeti yeşil parlar ve tıklanabilir indirme toast bildirimi gösterilir.
- **⚡ Listede Çift Tıklama ile Başlatma:** Oyun listesinde herhangi bir oyuna çift tıklandığında oyun doğrudan seçilip Discord üzerinde simülasyonu başlatılır.
- **🔄 Tüm Kaynakların Eşzamanlı Güncellenmesi:** Yenileme butonu artık Discord Detectable kataloğunun (3.000+) yanı sıra Steam Top 100 ve kütüphane kaynaklarını da tazeleyerek toast bildirimi sunar.
- **🔔 Modern Kayan Toast Bildirim Sistemi:** Oyun başlatma, katalog güncelleme ve yeni sürüm uyarıları için animasyonlu floating toast arayüzü eklendi.
- **🚀 CI/CD Release Otomasyonu:** GitHub Actions ile otomatik Portable ZIP ve Windows Setup Installer derleme iş akışı entegre edildi.

---

## [v1.1.0] - 2026-10-08

### ✨ Eklenenler
- Steam Top 100 ve Steam Yüklü Oyunlar entegrasyonu.
- Steam App ID ile özel oyun ekleme desteği.
- Çoklu tema ve Türkçe / İngilizce dil desteği.
- Akıllı exe algılama (launcher / crash-reporter filtreleme).

---

## [v1.0.0] - 2026-10-08

### 🚀 İlk Sürüm
- FakeLord Discord Oyun Simülatörü ilk kararlı sürüm.
- Ghost Process mimarisi ile sıfır CPU ve RAM kullanımı.
- Popüler rekabetçi oyunlar ve favoriler sistemi.
