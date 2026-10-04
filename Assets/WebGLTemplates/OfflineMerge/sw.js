/* Patched by ci/prepare-web.py after Unity export. */
const PREFIX = 'merge-sandbox:' + new URL(self.registration.scope).pathname + ':';
const CACHE = PREFIX + '__CACHE_VERSION__';
const ASSETS = __PRECACHE__;
self.addEventListener('install', event => event.waitUntil((async () => {
  const cache = await caches.open(CACHE);
  let next = 0, done = 0;
  const download = async () => {
    while (next < ASSETS.length) {
      const file = ASSETS[next++];
      const url = new URL(file, self.registration.scope).href;
      const response = await fetch(new Request(url, {cache:'reload'}));
      if (!response.ok) throw new Error('Offline download failed: ' + file);
      await cache.put(url, response);
      done++;
      for (const client of await self.clients.matchAll({includeUncontrolled:true})) client.postMessage({type:'cache-progress', done, total:ASSETS.length});
    }
  };
  const results = await Promise.allSettled([download(), download()]);
  if (results.some(result => result.status === 'rejected')) {
    await caches.delete(CACHE);
    throw new Error('The complete offline bundle could not be cached');
  }
  // No skipWaiting: an update must not replace assets beneath a running game.
})()));
self.addEventListener('activate', event => event.waitUntil((async () => {
  for (const key of await caches.keys()) if (key.startsWith(PREFIX) && key !== CACHE) await caches.delete(key);
  await self.clients.claim();
})()));
self.addEventListener('fetch', event => {
  if (event.request.method !== 'GET' || new URL(event.request.url).origin !== self.location.origin) return;
  event.respondWith((async () => {
    const cache = await caches.open(CACHE);
    const saved = await cache.match(event.request, {ignoreSearch:true});
    if (saved) return saved;
    if (event.request.mode === 'navigate') return await cache.match(new URL('index.html',self.registration.scope).href) || fetch(event.request);
    return fetch(event.request);
  })());
});
