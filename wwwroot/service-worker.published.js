// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');

const SW_VERSION = self.assetsManifest.version;
const CACHE_PREFIX = 'bibeli-';
const FRAMEWORK_CACHE = `${CACHE_PREFIX}framework-${SW_VERSION}`;
const DATA_CACHE = `${CACHE_PREFIX}data-v2`;
const OFFLINE_URL = '/';

self.addEventListener('install', event => {
    console.info('[SW] Install', SW_VERSION);
    // Take over immediately instead of waiting for all tabs to close.
    self.skipWaiting();
    event.waitUntil((async () => {
        // Pre-cache only the app shell + framework (NOT the 66 Bible books).
        const cache = await caches.open(FRAMEWORK_CACHE);
        const assets = self.assetsManifest.assets
            .filter(a => /\.(dll|wasm|html|js|css|woff2?|png|svg|ico|webmanifest|json)$/.test(a.url))
            .filter(a => !a.url.startsWith('data/books/'))
            .map(a => new Request(a.url, { integrity: a.hash, cache: 'no-cache' }));
        // addAll in small batches so one failure doesn't kill the install.
        const BATCH = 8;
        for (let i = 0; i < assets.length; i += BATCH) {
            try { await cache.addAll(assets.slice(i, i + BATCH)); }
            catch (e) { console.warn('[SW] batch failed', e); }
        }
        // Cache the verse-of-the-day file.
        try {
            const r = await fetch('/data/verset-du-jour.json');
            if (r.ok) await cache.put('/data/verset-du-jour.json', r.clone());
        } catch (e) {}
    })());
});

self.addEventListener('activate', event => {
    console.info('[SW] Activate', SW_VERSION);
    event.waitUntil((async () => {
        // Claim clients immediately.
        await self.clients.claim();
        // Delete obsolete caches.
        const keys = await caches.keys();
        await Promise.all(keys
            .filter(k => k.startsWith(CACHE_PREFIX) && k !== FRAMEWORK_CACHE && k !== DATA_CACHE)
            .map(k => caches.delete(k)));
    })());
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;
    const url = new URL(event.request.url);
    if (url.origin !== self.location.origin) return;

    // Bible data: cache-on-read, versioned data cache.
    if (url.pathname.startsWith('/data/')) {
        event.respondWith((async () => {
            const dataCache = await caches.open(DATA_CACHE);
            const cached = await dataCache.match(event.request);
            if (cached) return cached;
            try {
                const response = await fetch(event.request);
                if (response.ok) dataCache.put(event.request, response.clone());
                return response;
            } catch (err) {
                return new Response('', { status: 503, statusText: 'Offline' });
            }
        })());
        return;
    }

    // Navigation requests: serve index.html from cache (SPA fallback).
    if (event.request.mode === 'navigate') {
        event.respondWith((async () => {
            const cache = await caches.open(FRAMEWORK_CACHE);
            const cached = await cache.match('/index.html') || await cache.match('index.html');
            if (cached) return cached;
            try { return await fetch(event.request); }
            catch { return new Response('Offline', { status: 503 }); }
        })());
        return;
    }

    // Other assets: cache-first, then network.
    event.respondWith((async () => {
        const cache = await caches.open(FRAMEWORK_CACHE);
        const cached = await cache.match(event.request);
        if (cached) return cached;
        try {
            const response = await fetch(event.request);
            if (response.ok) cache.put(event.request, response.clone());
            return response;
        } catch (err) {
            return new Response('', { status: 503 });
        }
    })());
});

// Allow the page to trigger an update.
self.addEventListener('message', event => {
    if (event.data === 'SKIP_WAITING') self.skipWaiting();
    if (event.data === 'CLEAR_CACHE') {
        caches.keys().then(keys => Promise.all(keys.map(k => caches.delete(k))));
    }
});