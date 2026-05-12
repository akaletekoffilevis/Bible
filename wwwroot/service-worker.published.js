// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);

    // Cache all Bible data for full offline access
    try {
        const indexResponse = await fetch('/data/index.json');
        const books = await indexResponse.json();
        const dataUrls = ['/data/index.json'];
        for (const book of books) {
            dataUrls.push(`/data/books/${book.slug}.json`);
        }
        // Use a separate cache so main app cache isn't blocked on failure
        const dataCache = await caches.open('bible-data-v1');
        await dataCache.addAll(dataUrls);
        console.info(`Service worker: Cached ${books.length} Bible books for offline`);
    } catch (err) {
        console.warn('Service worker: Could not cache all Bible books, will cache on-demand', err);
        // Fallback: cache at least the index
        try {
            const dataCache = await caches.open('bible-data-v1');
            await dataCache.addAll(['/data/index.json']);
        } catch (e) {}
    }
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches (framework + data)
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => (key.startsWith(cacheNamePrefix) && key !== cacheName) || (key.startsWith('bible-data-') && key !== 'bible-data-v1'))
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        const url = new URL(event.request.url);

        // Bible data: check dedicated data cache first, fall back to network
        if (url.pathname.startsWith('/data/')) {
            const dataCache = await caches.open('bible-data-v1');
            cachedResponse = await dataCache.match(event.request);
            if (cachedResponse) return cachedResponse;
            try {
                const response = await fetch(event.request);
                if (response.ok) {
                    const cache = await caches.open(cacheName);
                    cache.put(event.request, response.clone());
                }
                return response;
            } catch (err) {
                return new Response('', { status: 503 });
            }
        }

        // For all navigation requests, try to serve index.html from cache
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
}
