import { initializeApp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js";
import { getMessaging, getToken, onMessage } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging.js";

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

// 🚀 ثبت Service Worker
navigator.serviceWorker.register('/js/firebase-messaging-sw.js', { type: "module" })
    .then((registration) => {
        console.log('✅ Service Worker ثبت شد:', registration);

        // بررسی وضعیت دسترسی به اعلان‌ها
        if (Notification.permission === "granted") {
            console.log("✅ دسترسی نوتیفیکیشن قبلاً داده شده.");
            requestFCMToken(messaging, registration);
        } else {
            console.log("🔔 درخواست مجوز نوتیفیکیشن...");
            Notification.requestPermission().then((permission) => {
                if (permission === "granted") {
                    requestFCMToken(messaging, registration);
                } else {
                    console.warn("⚠️ کاربر دسترسی نوتیفیکیشن را رد کرد.");
                }
            });
        }
    })
    .catch((error) => console.error("❌ خطا در ثبت Service Worker:", error));

// 📌 درخواست دریافت توکن از Firebase
function requestFCMToken(messaging, registration) {
    getToken(messaging, { serviceWorkerRegistration: registration })
        .then((token) => {
            if (token) {
                console.log("✅ توکن دریافت شد:", token);
                sendTokenToServer(token);
            } else {
                console.warn("⚠️ توکن دریافت نشد. مجوز اعلان‌ها بررسی شود.");
            }
        })
        .catch((err) => console.error("❌ خطا در دریافت توکن FCM:", err));
}

// 📌 ارسال توکن به سرور
function sendTokenToServer(token) {
    fetch("https://localhost:44335/api/v1/conversation/saveToken", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ deviceToken: token })
    })
        .then(response => response.json())
        .then(data => console.log("✅ توکن با موفقیت به سرور ارسال شد:", data))
        .catch(error => console.error("❌ خطا در ارسال توکن:", error));
}

