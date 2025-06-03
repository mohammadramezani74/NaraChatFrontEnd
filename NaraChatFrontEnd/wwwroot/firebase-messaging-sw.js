// ✅ فقط در Service Worker از نسخه‌های compat استفاده کن
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js');

// 🔑 مقداردهی اولیه پروژه Firebase
firebase.initializeApp({
    apiKey: "AIzaSyCDok7ESoslSXEOqXATLyLsiTOd-IGTQ_k",
    authDomain: "chatapp-7573b.firebaseapp.com",
    projectId: "chatapp-7573b",
    storageBucket: "chatapp-7573b.appspot.com",
    messagingSenderId: "349476543753",
    appId: "1:349476543753:web:3650974815b119368b87d8",
    measurementId: "G-0G8YGN3M03"
});

// ⚙️ فعال‌سازی Messaging
const messaging = firebase.messaging();

// ✅ هندل کردن نوتیفیکیشن‌های پس‌زمینه (اختیاری)
messaging.onBackgroundMessage(function (payload) {
    console.log('[firebase-messaging-sw.js] Received background message ', payload);
    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: '/firebase-logo.png' // یا هر آیکن دیگه
    };
    console.log(notificationTitle);
    console.log(notificationOptions);

    self.registration.showNotification(notificationTitle, notificationOptions);
});
