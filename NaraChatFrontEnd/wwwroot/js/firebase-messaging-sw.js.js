import { initializeApp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js";
import { getMessaging, onBackgroundMessage } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-sw.js";

// 📌 پیکربندی Firebase
const firebaseConfig = {
    apiKey: "AIzaSyCDok7ESoslSXEOqXATLyLsiTOd-IGTQ_k",
    authDomain: "chatapp-7573b.firebaseapp.com",
    projectId: "chatapp-7573b",
    storageBucket: "chatapp-7573b.appspot.com",
    messagingSenderId: "349476543753",
    appId: "1:349476543753:web:3650974815b119368b87d8",
    measurementId: "G-0G8YGN3M03"
};

// مقداردهی اولیه Firebase
const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// 📩 هندل پیام‌های پس‌زمینه (Background Notifications)
onBackgroundMessage(messaging, (payload) => {
    console.log("📩 دریافت پیام در پس‌زمینه:", payload);

    self.registration.showNotification(payload.notification.title, {
        body: payload.notification.body,
        icon: "/icon.png" // مسیر آیکن نوتیفیکیشن
    });
});
