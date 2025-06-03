import { initializeApp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js";
import { getMessaging, getToken } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging.js";

const firebaseConfig = {
    apiKey: "AIzaSyCDok7ESoslSXEOqXATLyLsiTOd-IGTQ_k",
    authDomain: "chatapp-7573b.firebaseapp.com",
    projectId: "chatapp-7573b",
    storageBucket: "chatapp-7573b.appspot.com",
    messagingSenderId: "349476543753",
    appId: "1:349476543753:web:3650974815b119368b87d8",
    measurementId: "G-0G8YGN3M03"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

window.requestPermission = async function () {

    try {
        const permission = await Notification.requestPermission();
        if (permission === 'granted') {
            const token = await getToken(messaging, {
                vapidKey: "BGqk9Wa2aoG7OQ-llhSn7u7TcQq7W-y7bz9y5BH_PoCQc3TmvjsPCm6FFBDCasRHHiTI2idAlEu6tWkUW_iHYms"
            });
            console.log("✅ FCM Token:", token);
            return token;
        } else {
            console.warn("❌ Notification permission denied.");
            return null;
        }
    } catch (err) {
        console.error("❌ Error in requestPermission:", err);
        return null;
    }
};

