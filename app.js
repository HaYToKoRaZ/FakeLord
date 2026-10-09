/**
 * FakeLord Showcase Web Engine
 * Bilingual (TR/EN) with URL Query (?lang=tr|en), Mascot "Çakıl", 
 * Theme Factory 12 Palettes & Discord Live Simulator.
 */

// --- 1. TRANSLATION DICTIONARY ---
const translations = {
  tr: {
    navFeatures: "Özellikler",
    navSimulator: "Discord Simülatörü",
    navHow: "Nasıl Çalışır?",
    navFaq: "S.S.S.",
    navDownload: "İndir",
    heroBadge: "Windows 10/11 Uyumlu • Resmi Sürüm",
    heroTitle: "FakeLord — Discord Hayalet Oyun & <span class=\"hero-gradient\">Aktivite Simülatörü</span>",
    heroSubtitle: "3.000+ resmi Discord oyunu ve Steam Top 100 kütüphanesini tek tıkla arka planda simüle edin. Sıfır CPU tüketimi, %100 ban riski yok, şifre ve token gerekmez.",
    heroCtaDownload: "Hemen İndir",
    heroCtaSource: "GitHub Kaynak Kodu",
    metaZeroCpu: "%0 CPU • 5MB RAM",
    metaSafe: "%100 Ban Korumalı",
    metaNoToken: "Hesap / Şifre İstemez",
    mascotTag: "RESMİ MASKOT: ÇAKIL",
    mascotBubbleTitle: "Çakıl Konuşuyor",
    mascotPetBtn: "🐾 Çakıl'ı Sev",
    mascotTipBtn: "💡 İpucu İste",
    navTheme: "Tema",
    footerLinksTitle: "Bağlantılar",
    footerContactTitle: "İletişim & Geliştirici",
    simBadge: "CANLI ETKİLEŞİMLİ ÖNİZLEME",
    simTitle: "Discord Profil Kartı Simülatörü",
    simDesc: "Fakelord ile oyun başlattığınızda arkadaşlarınızın Discord profilinizde göreceği canlı Rich Presence kartını hemen test edin.",
    simPickerTitle: "Simüle Edilecek Oyunu Seçin:",
    simPlaying: "Bir Oyun Oynuyor",
    simVerified: "Doğrulanmış Oyun",
    simInfo: "💡 <strong>Nasıl çalışır?</strong> Fakelord, Win32 Ghost Process motoru sayesinde <code>csgo.exe</code> veya <code>GTA5.exe</code> gibi süreçleri penceresiz ve %0 CPU ile çalıştırır. Discord bu süreci resmi doğrulanmış oyun olarak otomatik yakalar.",
    featBadge: "MİMARİ VE PERFORMANS",
    featTitle: "Neden FakeLord Tercih Etmelisiniz?",
    featDesc: "Geleneksel RPC araçlarının aksine, doğrudan Discord'un kendi oyun algılama motoruna uyumlu çalışan ilk hafif masaüstü çözümü.",
    feat1Title: "⚡ Sıfır Kaynak Tüketimi",
    feat1Desc: "Ghost Process teknolojisi sayesinde arka planda uyur. %0 CPU harcar ve 5 MB'tan daha az RAM tüketir. Oyun performansınızı asla etkilemez.",
    feat2Title: "🛡️ %100 Güvenli & Ban Riski Yok",
    feat2Desc: "Discord tokenı, kullanıcı şifresi veya hafıza enjeksiyonu kullanmaz. Discord'un resmi oyun algılama mekanizmasıyla doğal çalışır.",
    feat3Title: "🎮 3.000+ Doğrulanmış Oyun",
    feat3Desc: "Discord Detectable Games kataloğu ve Steam Top 100 ile senkronize. Popüler tüm oyunlar resmi afiş ve doğrulanmış rozetle görünür.",
    feat4Title: "🎨 12 Theme Factory Teması",
    feat4Desc: "Discord Nitro, Midnight Galaxy, Tech Innovation, Ocean Depths ve 8 farklı özel paletle arayüzünüzü dilediğiniz gibi özelleştirin.",
    feat5Title: "🔤 A-Z Hızlı Harf Şeridi",
    feat5Desc: "Gelişmiş alfabetik şerit ve klavye kısayolları ile binlerce oyun arasında tek saniyede istediğiniz harfe ve oyuna zıplayın.",
    feat6Title: "🚀 Steam App ID Desteği",
    feat6Desc: "Listede olmayan özel veya bağımsız oyunları tek tıkla Steam App ID girerek ekleyin ve anında simüle edin.",
    howBadge: "3 BASİT ADIM",
    howTitle: "Nasıl Kullanılır?",
    howDesc: "Kurulum gerekmez. İndirin, açın ve oynamaya başlayın.",
    step1Title: "1. Oyunu Seçin",
    step1Desc: "3.000+ oyun arasından arama yapın, harf şeridini kullanın veya Steam App ID ile yeni oyun ekleyin.",
    step2Title: "2. 'Oyna' Butonuna Basın",
    step2Desc: "Tek tıkla arka plan hayalet süreci başlasın. Ekranda can sıkıcı pencereler açılmaz.",
    step3Title: "3. Discord'da Keyfini Çıkarın",
    step3Desc: "Discord profilinizde oyununuz anında doğrulanmış afiş ve yeşil durumla parıldasın!",
    faqBadge: "MERAK EDİLENLER",
    faqTitle: "Sıkça Sorulan Sorular",
    faqDesc: "FakeLord hakkında en çok sorulan soruların yanıtları.",
    faq1Q: "Discord hesabımdan ban yer miyim?",
    faq1A: "Kesinlikle hayır! FakeLord hiçbir şekilde Discord API token'ı, hesap şifresi veya bellek manipülasyonu (injection) kullanmaz. Discord'un 'Şu anda oynuyor' mekanizması Windows'ta çalışan .exe süreç adlarını okuyarak çalışır. FakeLord yalnızca bu süreci arka planda açar.",
    faq2Q: "Bilgisayarımı yavaşlatır mı?",
    faq2A: "Asla yavaşlatmaz. Hayalet süreçler tamamen uykuda (Thread.Sleep) bekletilir. CPU kullanımı %0, RAM tüketimi ise yalnızca 3-5 MB civarındadır.",
    faq3Q: "Uygulamayı kapattığımda oyun devam eder mi?",
    faq3A: "Fakelord'u kapattığınızda veya 'Durdur' dediğinizde, arkadaki tüm hayalet süreçler Win32 TerminateProcess ile anında güvenle temizlenir. Arkada hiçbir zombi süreç kalmaz.",
    faq4Q: "Özel veya indie oyunları ekleyebilir miyim?",
    faq4A: "Evet! Steam App ID kutusuna oyunun Steam mağaza numarasını girip 'Özel Ekle' butonuna basmanız yeterlidir. Uygulama oyun adını ve simgesini otomatik çeker.",
    ctaTitle: "Discord Deneyiminizi Bugün Zirveye Taşıyın",
    ctaSubtitle: "FakeLord'u hemen ücretsiz indirin veya GitHub üzerinden açık kaynak projemize yıldız verin.",
    ctaDownloadBtn: "Ücretsiz İndir (Windows 10/11)",
    ctaGithubBtn: "⭐ GitHub'da İncele",
    ctaNote: "Taşınabilir (Portable) • Kurulum Gerektirmez • Açık Kaynak",
    footerAbout: "Fakelord, Discord ve Steam ekosistemi için tasarlanmış yüksek performanslı, hayalet süreç tabanlı modern oyun simülasyon aracıdır.",
    footerDev: "Geliştirici:",
    footerRights: "Tüm hakları saklıdır. Discord Inc. veya Valve Corp. ile resmi bağı bulunmamaktadır.",
    error404Title: "Sayfa Hayalet Moduna Geçti!",
    error404Desc: "Aradığınız sayfa silinmiş, adı değiştirilmiş veya geçici olarak ulaşılamıyor olabilir. FakeLord ile arka planda oyun oynamaya devam edebilirsiniz!",
    error404Quote: "Miyav? Bu sayfa kaybolmuş gibi görünüyor... Belki de hayalet moduna (Ghost Mode) geçmiştir! 🐾",
    errorHomeBtn: "🏠 Ana Sayfaya Dön",
    errorPetBtn: "🐾 Çakıl'ı Sev",
    cakilQuotes: [
      "Miyav! Hoş geldin! Ben Çakıl, FakeLord'un gamer maskotuyum 🐾",
      "Biliyor musun? FakeLord arkada çalışırken sadece 5 MB RAM tüketir, benim kedi mamamdan bile az! 😸",
      "Kafamı kaşıdığın için teşekkürler! Hadi Discord'da biraz GTA V oynuyor görünelim 🎮",
      "A-Z harf şeridini denedin mi? Binlerce oyun arasında tek tıkla zıplayabiliyorsun! ⚡",
      "İstediğin temayı yukarıdaki renk paletinden seçebilirsin, benim tüylerim her renge yakışır! ✨",
      "Token yok, şifre yok, ban riski sıfır! Güvenle rol yapmanın tadını çıkar miyav~ 🛡️"
    ]
  },
  en: {
    navFeatures: "Features",
    navSimulator: "Discord Simulator",
    navHow: "How it Works",
    navFaq: "FAQ",
    navDownload: "Download",
    heroBadge: "Windows 10/11 Ready • Official Release",
    heroTitle: "FakeLord — Discord Ghost Game & <span class=\"hero-gradient\">Activity Simulator</span>",
    heroSubtitle: "Simulate 3,000+ official Discord detectable games and Steam Top 100 titles with one click. Zero CPU overhead, 100% ban-proof, no passwords or tokens required.",
    heroCtaDownload: "Download Now",
    heroCtaSource: "GitHub Source Code",
    metaZeroCpu: "0% CPU • 5MB RAM",
    metaSafe: "100% Ban-Proof",
    metaNoToken: "No Account / Token Needed",
    mascotTag: "OFFICIAL MASCOT: ÇAKIL",
    mascotBubbleTitle: "Çakıl is Speaking",
    mascotPetBtn: "🐾 Pet Çakıl",
    mascotTipBtn: "💡 Get a Tip",
    navTheme: "Theme",
    footerLinksTitle: "Links",
    footerContactTitle: "Contact & Developer",
    simBadge: "LIVE INTERACTIVE PREVIEW",
    simTitle: "Discord Profile Card Simulator",
    simDesc: "Preview the exact Rich Presence status card your friends will see on your Discord profile when playing games through FakeLord.",
    simPickerTitle: "Select a Game to Simulate:",
    simPlaying: "Playing a Game",
    simVerified: "Verified Game",
    simInfo: "💡 <strong>How it works:</strong> FakeLord uses Win32 Ghost Process architecture to launch process templates like <code>csgo.exe</code> or <code>GTA5.exe</code> without windows and with 0% CPU. Discord natively detects the game with official badges.",
    featBadge: "ARCHITECTURE & PERFORMANCE",
    featTitle: "Why Choose FakeLord?",
    featDesc: "Unlike traditional RPC spoofers, FakeLord natively interops with Discord's built-in process detection engine.",
    feat1Title: "⚡ Zero Resource Usage",
    feat1Desc: "Sleeps quietly in the background via Ghost Process tech. Consumes 0% CPU and less than 5 MB of RAM, leaving your game FPS untouched.",
    feat2Title: "🛡️ 100% Safe & Ban-Proof",
    feat2Desc: "Zero memory injection, zero user tokens, zero passwords. Runs cleanly alongside Discord using official OS process recognition.",
    feat3Title: "🎮 3,000+ Verified Titles",
    feat3Desc: "Fully synchronized with Discord's Detectable database and Steam Top 100. Display official artwork and verified badges.",
    feat4Title: "🎨 12 Theme Factory Palettes",
    feat4Desc: "Customize with Discord Nitro, Midnight Galaxy, Tech Innovation, Ocean Depths, and 8 additional curated colorways.",
    feat5Title: "🔤 A-Z Quick Jump Ribbon",
    feat5Desc: "Jump to any letter instantly using the alphabetical bar and keyboard shortcuts across thousands of catalog games.",
    feat6Title: "🚀 Steam App ID Support",
    feat6Desc: "Easily add custom or indie titles by entering their Steam App ID for automatic metadata, title, and artwork fetching.",
    howBadge: "3 EASY STEPS",
    howTitle: "How to Use?",
    howDesc: "No complex setup. Just download, launch, and pick your game.",
    step1Title: "1. Pick a Game",
    step1Desc: "Search from 3,000+ games, use the alphabetical ribbon, or add any custom Steam App ID.",
    step2Title: "2. Click 'Play'",
    step2Desc: "One click launches the background ghost process without any disruptive open windows.",
    step3Title: "3. Enjoy on Discord",
    step3Desc: "Your Discord profile immediately shines with the verified game status and live activity!",
    faqBadge: "FREQUENT QUESTIONS",
    faqTitle: "Frequently Asked Questions",
    faqDesc: "Answers to common inquiries regarding FakeLord's operation.",
    faq1Q: "Can my Discord account get banned?",
    faq1A: "Absolutely not! FakeLord does not interact with Discord's API, tokens, or network packets. Discord natively detects running Win32 processes on Windows. FakeLord simply provides a stealth background process.",
    faq2Q: "Will it slow down my PC?",
    faq2A: "Not at all. The ghost processes remain in sleep state (Thread.Sleep). They consume 0% CPU and only about 3-5 MB of memory.",
    faq3Q: "Does the game stop when I close FakeLord?",
    faq3A: "Yes! When you close FakeLord or click 'Stop', all background ghost processes are instantly and cleanly terminated via Win32 TerminateProcess. Zero zombie processes.",
    faq4Q: "Can I add indie or unlisted games?",
    faq4A: "Yes! Simply paste the game's Steam App ID into the Custom Game box and click 'Add'. It fetches the game title and icon automatically.",
    ctaTitle: "Elevate Your Discord Presence Today",
    ctaSubtitle: "Download FakeLord for free or check out our open-source repository on GitHub.",
    ctaDownloadBtn: "Download Free (Windows 10/11)",
    ctaGithubBtn: "⭐ Star on GitHub",
    ctaNote: "Portable • Zero Installation • Open Source",
    footerAbout: "FakeLord is a high-performance, ghost-process game presence spoofer designed for Discord and Steam users.",
    footerDev: "Developer:",
    footerRights: "All rights reserved. Not affiliated with Discord Inc. or Valve Corp.",
    error404Title: "Page Entered Ghost Mode!",
    error404Desc: "The page you are looking for might have been removed, had its name changed, or is temporarily unavailable. Keep playing stealth games with FakeLord!",
    error404Quote: "Meow? Looks like this page is lost... Maybe it entered Ghost Mode! 🐾",
    errorHomeBtn: "🏠 Back to Home",
    errorPetBtn: "🐾 Pet Çakıl",
    cakilQuotes: [
      "Meow! Welcome! I'm Çakıl, FakeLord's gaming mascot 🐾",
      "Did you know? FakeLord uses only 5 MB RAM in the background—less than a bite of cat treats! 😸",
      "Thanks for the head pats! Ready to rock GTA V on Discord all day! 🎮",
      "Have you tried the A-Z quick jump ribbon? Jump across thousands of games in a snap! ⚡",
      "Pick your favorite theme from the palette up top—my fur looks great in every color! ✨",
      "No tokens, no passwords, 100% ban-proof! Enjoy your roleplay in peace meow~ 🛡️"
    ]
  }
};

// --- 2. GAME CATALOG FOR SIMULATOR ---
const simulatorGames = [
  { id: "gta5", title: "Grand Theft Auto V", icon: "assets/gta5.webp", sub: "Playing in Los Santos", exe: "GTA5.exe" },
  { id: "cs2", title: "Counter-Strike 2", icon: "assets/cs2.webp", sub: "Competitive • Mirage", exe: "cs2.exe" },
  { id: "lol", title: "League of Legends", icon: "assets/lol.webp", sub: "Ranked Solo/Duo • Summoner's Rift", exe: "League of Legends.exe" },
  { id: "valorant", title: "VALORANT", icon: "assets/valorant.webp", sub: "In Match • Ascent (12-11)", exe: "VALORANT.exe" },
  { id: "cyberpunk", title: "Cyberpunk 2077", icon: "assets/cyberpunk.webp", sub: "Exploring Night City", exe: "Cyberpunk2077.exe" }
];

// --- 3. STATE ---
let currentLang = "tr";
let currentTheme = "discord";
let selectedSimGame = simulatorGames[0];
let simTimerSeconds = 42;
let simTimerInterval = null;
let quoteIndex = 0;

// --- 4. LANGUAGE HANDLING & URL PARAMS ---
function getInitialLang() {
  const urlParams = new URLSearchParams(window.location.search);
  const langParam = urlParams.get("lang");
  if (langParam && (langParam === "tr" || langParam === "en")) {
    return langParam;
  }
  const saved = localStorage.getItem("fakelord_lang");
  if (saved && (saved === "tr" || saved === "en")) {
    return saved;
  }
  const browserLang = navigator.language || navigator.userLanguage;
  return (browserLang && browserLang.startsWith("tr")) ? "tr" : "en";
}

function setLanguage(lang, updateUrl = true) {
  if (lang !== "tr" && lang !== "en") return;
  currentLang = lang;
  localStorage.setItem("fakelord_lang", lang);

  // Update HTML lang attribute
  document.documentElement.lang = lang;

  // Update URL search query
  if (updateUrl) {
    const url = new URL(window.location);
    url.searchParams.set("lang", lang);
    window.history.replaceState({}, "", url.toString());
  }

  // Update internal anchor links so user clicks stay on the same language
  updateLinksWithLang(lang);

  // Apply translations to DOM
  applyTranslations();

  // Update Active Button in Header
  document.querySelectorAll(".lang-btn").forEach(btn => {
    btn.classList.toggle("active", btn.dataset.lang === lang);
  });
}

function updateLinksWithLang(lang) {
  document.querySelectorAll("a[href]").forEach(link => {
    const href = link.getAttribute("href");
    if (!href || href.startsWith("http") || href.startsWith("mailto:") || href.startsWith("#")) {
      return;
    }
    try {
      const url = new URL(link.href, window.location.origin);
      url.searchParams.set("lang", lang);
      link.href = url.pathname + url.search + url.hash;
    } catch (e) {
      // Ignore
    }
  });
}

function applyTranslations() {
  const t = translations[currentLang];
  document.querySelectorAll("[data-i18n]").forEach(el => {
    const key = el.dataset.i18n;
    if (t[key]) {
      el.innerHTML = t[key];
    }
  });

  // Mascot bubble current quote
  updateMascotBubble();
}

// --- 5. THEME SWITCHING (12 THEMES) ---
function setTheme(theme, isUserAction = false) {
  currentTheme = theme;
  document.documentElement.setAttribute("data-theme", theme);
  localStorage.setItem("fakelord_theme", theme);

  document.querySelectorAll(".theme-opt").forEach(btn => {
    btn.classList.toggle("active", btn.dataset.themeVal === theme);
  });

  // Update active dot in theme button
  const activeOpt = document.querySelector(`.theme-opt[data-theme-val="${theme}"]`);
  if (activeOpt) {
    const color = activeOpt.querySelector(".theme-opt-color")?.style.background;
    const dot = document.querySelector(".theme-btn .theme-dot");
    if (dot && color) dot.style.background = color;
  }

  // Only react to explicit user clicks, avoid reflow on silent init
  if (isUserAction) {
    triggerCakilThemeReaction();
  }
}

// --- 6. MASCOT "ÇAKIL" INTERACTIONS ---
function updateMascotBubble(customText = null) {
  const bubbleText = document.getElementById("cakilBubbleText");
  if (!bubbleText) return;
  if (customText) {
    bubbleText.textContent = customText;
  } else {
    const quotes = translations[currentLang].cakilQuotes;
    bubbleText.textContent = quotes[quoteIndex % quotes.length];
  }
}

function petCakil() {
  playCuteBeep();
  quoteIndex++;
  const petQuotes = currentLang === "tr"
    ? ["Mırrrrr! ❤️ Kafamı kaşıdın! Teşekkürler dostum! 🐾", "Miyav! Sevgi dolu bir paticik sana geliyor! 😻"]
    : ["Purrrr! ❤️ Thanks for the sweet head pat, buddy! 🐾", "Meow! Sending you a warm gaming paw! 😻"];
  const selected = petQuotes[Math.floor(Math.random() * petQuotes.length)];
  updateMascotBubble(selected);

  // GPU compositor accelerated animation (Zero layout reflow)
  const wrap = document.querySelector(".mascot-visual-wrap");
  if (wrap) {
    wrap.classList.add("pet-anim");
    setTimeout(() => { wrap.classList.remove("pet-anim"); }, 300);
  }
}

function getCakilTip() {
  playCuteBeep();
  quoteIndex++;
  updateMascotBubble();
}

function triggerCakilThemeReaction() {
  const reaction = currentLang === "tr"
    ? `Miyav! Yeni temaya bayıldım, tüylerimle harika uyum sağladı! ✨`
    : `Meow! Love this new palette, it matches my gaming vibe perfectly! ✨`;
  updateMascotBubble(reaction);
}

// Sound Synthesizer via Web Audio API (Zero dependencies, pure Web Audio)
function playCuteBeep() {
  try {
    const ctx = new (window.AudioContext || window.webkitAudioContext)();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = "sine";
    osc.frequency.setValueAtTime(587.33, ctx.currentTime); // D5
    osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.15); // A5
    gain.gain.setValueAtTime(0.08, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.25);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start();
    osc.stop(ctx.currentTime + 0.25);
  } catch (e) {
    // Audio Context blocked or unavailable, safe fail
  }
}

// --- 7. DISCORD SIMULATOR CONTROLS ---
function updateSimulator(game) {
  selectedSimGame = game;
  const gameTitleEl = document.getElementById("simGameTitle");
  const gameSubEl = document.getElementById("simGameSub");
  const gameIconEl = document.getElementById("simGameIcon");

  if (gameTitleEl) gameTitleEl.textContent = game.title;
  if (gameSubEl) gameSubEl.textContent = game.sub;
  if (gameIconEl) {
    gameIconEl.innerHTML = `<img src="${game.icon}" alt="${game.title}" id="simGameImg">`;
  }

  document.querySelectorAll(".game-pill").forEach(p => {
    p.classList.toggle("active", p.dataset.gameId === game.id);
  });
}

function startSimTimer() {
  if (simTimerInterval) clearInterval(simTimerInterval);
  const timeEl = document.getElementById("simGameTime");
  simTimerInterval = setInterval(() => {
    simTimerSeconds++;
    const mins = Math.floor(simTimerSeconds / 60).toString().padStart(2, "0");
    const secs = (simTimerSeconds % 60).toString().padStart(2, "0");
    if (timeEl) {
      timeEl.textContent = `${mins}:${secs} ${currentLang === "tr" ? "geçti" : "elapsed"}`;
    }
  }, 1000);
}

// --- 8. FAQ ACCORDION ---
function initFaq() {
  document.querySelectorAll(".faq-item").forEach(item => {
    const btn = item.querySelector(".faq-question");
    btn.addEventListener("click", () => {
      const isOpen = item.classList.contains("open");
      document.querySelectorAll(".faq-item").forEach(i => i.classList.remove("open"));
      if (!isOpen) {
        item.classList.add("open");
      }
    });
  });
}

// --- 9. INITIALIZATION ---
document.addEventListener("DOMContentLoaded", () => {
  // 1. Language Init
  const initialLang = getInitialLang();
  setLanguage(initialLang, false);

  document.querySelectorAll(".lang-btn").forEach(btn => {
    btn.addEventListener("click", (e) => {
      e.preventDefault();
      setLanguage(btn.dataset.lang, true);
    });
  });

  // 2. Theme Init
  const savedTheme = localStorage.getItem("fakelord_theme") || "discord";
  setTheme(savedTheme, false);

  const themeToggle = document.getElementById("themeDropdownToggle");
  const themeDropdown = document.getElementById("themeDropdown");
  if (themeToggle && themeDropdown) {
    themeToggle.addEventListener("click", (e) => {
      e.stopPropagation();
      themeDropdown.classList.toggle("open");
    });
    document.addEventListener("click", () => {
      themeDropdown.classList.remove("open");
    });
  }

  document.querySelectorAll(".theme-opt").forEach(opt => {
    opt.addEventListener("click", () => {
      setTheme(opt.dataset.themeVal, true);
      if (themeDropdown) themeDropdown.classList.remove("open");
    });
  });

  // 3. Mascot Events
  const petBtn = document.getElementById("cakilPetBtn");
  const tipBtn = document.getElementById("cakilTipBtn");
  const mascotImg = document.getElementById("cakilMascotImg");
  if (petBtn) petBtn.addEventListener("click", petCakil);
  if (tipBtn) tipBtn.addEventListener("click", getCakilTip);
  if (mascotImg) mascotImg.addEventListener("click", petCakil);

  // 4. Simulator Init
  document.querySelectorAll(".game-pill").forEach(pill => {
    pill.addEventListener("click", () => {
      const g = simulatorGames.find(x => x.id === pill.dataset.gameId);
      if (g) updateSimulator(g);
    });
  });
  startSimTimer();

  // 5. FAQ Init
  initFaq();
});

// 6. Service Worker Registration (Long-Term Browser Caching & Instant Loads)
if ("serviceWorker" in navigator) {
  window.addEventListener("load", () => {
    navigator.serviceWorker.register("sw.js").catch(() => {});
  });
}
