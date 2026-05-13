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
    // Uses individual fetches instead of addAll to avoid:
    //   - Single-file failure killing the entire batch
    //   - Browser timeout with 67 URLs / 43 MB
    const dataCache = await caches.open('bible-data-v1');
    let cachedCount = 0;
    let failCount = 0;

    // 1. Cache index.json first (critical, small)
    try {
        const idxResp = await fetch('/data/index.json');
        if (idxResp.ok) {
            await dataCache.put('/data/index.json', idxResp);
            cachedCount++;
        }
    } catch (e) {
        console.warn('Service worker: Could not cache index.json', e);
        failCount++;
    }

    // 2. Parse index to get book list (from network or cache)
    let books = [];
    try {
        const idxResp2 = await dataCache.match('/data/index.json') || await fetch('/data/index.json');
        books = await (idxResp2 instanceof Response ? idxResp2 : idxResp2).json();
    } catch (e) {
        console.warn('Service worker: Could not read book index, skipping book caching', e);
    }

    // 3. Cache books individually in small batches (5 at a time)
    //    so a single failure doesn't block everything
    if (books.length > 0) {
        const BATCH_SIZE = 5;
        for (let i = 0; i < books.length; i += BATCH_SIZE) {
            const batch = books.slice(i, i + BATCH_SIZE);
            await Promise.all(batch.map(async (book) => {
                const url = `/data/books/${book.slug}.json`;
                try {
                    const resp = await fetch(url);
                    if (resp.ok) {
                        await dataCache.put(url, resp);
                        cachedCount++;
                    } else {
                        failCount++;
                    }
                } catch (e) {
                    failCount++;
                    console.warn(`Service worker: Could not cache ${url}`, e);
                }
            }));
        }
        console.info(`Service worker: Cached ${cachedCount}/${books.length + 1} Bible files for offline (${failCount} failed)`);
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
