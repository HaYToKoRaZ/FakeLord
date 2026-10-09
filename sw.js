// FakeLord Service Worker - High-Performance Cache-First Engine
const CACHE_NAME = 'fakelord-v1.7';
const PRECACHE_ASSETS = [
  './',
  'index.html',
  '404.html',
  'styles/main.css',
  'app.js',
  'site.webmanifest',
  'favicon.ico',
  'assets/icon.webp',
  'assets/cakil.webp',
  'assets/cakil.jpg',
  'assets/gta5.webp',
  'assets/cs2.webp',
  'assets/lol.webp',
  'assets/valorant.webp',
  'assets/cyberpunk.webp',
  'assets/flag_tr.png',
  'assets/flag_gb.png'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(PRECACHE_ASSETS))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys =>
      Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
    ).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET') return;
  
  // Stale-While-Revalidate strategy for optimal cache performance and freshness
  event.respondWith(
    caches.match(event.request).then(cachedResponse => {
      const fetchPromise = fetch(event.request).then(networkResponse => {
        if (networkResponse && networkResponse.status === 200 && networkResponse.type === 'basic') {
          const responseToCache = networkResponse.clone();
          caches.open(CACHE_NAME).then(cache => cache.put(event.request, responseToCache));
        }
        return networkResponse;
      }).catch(() => cachedResponse);

      return cachedResponse || fetchPromise;
    })
  );
});
