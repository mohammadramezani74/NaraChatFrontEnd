//const CACHE_NAME = `nara-chat-cache-${new Date().getTime()}`;

//const CACHE_TTL = 24 * 60 * 60 * 1000; // 24 ساعت

//const urlsToCache = [
//    '/',
//    '/index.html',
//    '/manifest.json',
//    '/icon-192.png',
//    '/icon-512.png'
//];
//self.addEventListener('activate', (event) => {
//    event.waitUntil(
//        caches.keys().then((cacheNames) => {
//            return Promise.all(
//                cacheNames.map((cacheName) => {
//                    if (cacheName !== CACHE_NAME) {
//                        console.log('Removing old cache:', cacheName);
//                        return caches.delete(cacheName);
//                    }
//                })
//            );
//        })
//    );

//    self.clients.claim().then(() => {
//        self.clients.matchAll({ type: 'window' }).then((clients) => {
//            clients.forEach((client) => client.navigate(client.url));
//        });
//    });
//});
//// نصب سرویس‌ورکر و کش کردن فایل‌های ضروری
//self.addEventListener('install', (event) => {
//    event.waitUntil(
//        caches.open(CACHE_NAME)
//            .then((cache) => {
//                console.log('Opened cache');
//                return cache.addAll(urlsToCache);
//            })
//            .catch((error) => console.error('Cache open failed:', error))
//    );
//    self.skipWaiting();
//});

//// فعال‌سازی سرویس‌ورکر و حذف کش‌های قدیمی
//self.addEventListener('activate', (event) => {
//    event.waitUntil(
//        caches.keys().then((cacheNames) => {
//            return Promise.all(
//                cacheNames.map((cacheName) => {
//                    if (cacheName !== CACHE_NAME) {
//                        console.log('Removing old cache:', cacheName);
//                        return caches.delete(cacheName);
//                    }
//                })
//            );
//        })
//    );
//    self.clients.claim();
//});

//// مدیریت درخواست‌ها با استراتژی Network First
//self.addEventListener('fetch', (event) => {
//    if (event.request.method !== 'GET') return; // کش فقط برای درخواست‌های GET

//    const { destination } = event.request;
//    const isStatic = ['document', 'script', 'style', 'image', 'font'].includes(destination);

//    if (!isStatic) return; // فقط فایل‌های استاتیک کش شوند

//    event.respondWith(
//        fetch(event.request)
//            .then((response) => {
//                return caches.open(CACHE_NAME).then((cache) => {
//                    cache.put(event.request, response.clone());
//                    return response;
//                });
//            })
//            .catch(() => caches.match(event.request))
//    );
//});

//// حذف کش‌های قدیمی بر اساس TTL
//setInterval(() => {
//    caches.open(CACHE_NAME).then((cache) => {
//        cache.keys().then((keys) => {
//            keys.forEach((request) => {
//                cache.match(request).then((response) => {
//                    if (!response) return;
//                    const date = response.headers.get('date');
//                    if (date && (Date.now() - new Date(date).getTime() > CACHE_TTL)) {
//                        cache.delete(request);
//                    }
//                });
//            });
//        });
//    });
//}, CACHE_TTL);
self.addEventListener('install', (event) => {
    // بلافاصله نصب و آماده‌سازی سرویس ورکر بدون انتظار
    self.skipWaiting();
});

// فعال‌سازی سرویس ورکر
self.addEventListener('activate', (event) => {
    event.waitUntil(
        // تسلط گرفتن بر کلاینت‌های فعال
        self.clients.claim()
    );
});

// مدیریت درخواست‌ها بدون استفاده از کش
self.addEventListener('fetch', (event) => {
    event.respondWith(
        // به طور مستقیم درخواست‌ها را به شبکه ارسال می‌کنیم
        fetch(event.request)
    );
});
