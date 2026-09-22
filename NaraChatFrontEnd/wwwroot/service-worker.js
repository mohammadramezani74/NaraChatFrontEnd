
self.addEventListener('install', event => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil(
        self.clients.claim()
    );
});

//// مدیریت درخواست‌ها بدون استفاده از کش
//self.addEventListener('fetch', (event) => {
//    event.respondWith(
//        // به طور مستقیم درخواست‌ها را به شبکه ارسال می‌کنیم
//        fetch(event.request)
//    );
//});
