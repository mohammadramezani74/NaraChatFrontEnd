let mediaRecorder;
let audioChunks = [];

window.startRecording = async function () {
    let stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    mediaRecorder = new MediaRecorder(stream);

    mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
            audioChunks.push(event.data);
        }
    };

    mediaRecorder.onstop = async () => {
        let audioBlob = new Blob(audioChunks, { type: "audio/wav" });

        let reader = new FileReader();
        reader.readAsDataURL(audioBlob);
        reader.onloadend = () => {
            let base64Data = reader.result.split(',')[1]; // حذف قسمت data URL
            DotNet.invokeMethod("NaraChatFrontEnd.Pages.ChatPages", "ReceiveAudioFile", base64Data);
        };

        let audioUrl = URL.createObjectURL(audioBlob);
        document.getElementById("audioPlayer").src = audioUrl;
        audioChunks = [];
    };

    mediaRecorder.start();
};

window.stopRecording = function () {
    return new Promise((resolve, reject) => {
        if (mediaRecorder && mediaRecorder.state !== "inactive") {
            mediaRecorder.onstop = () => {
                let audioBlob = new Blob(audioChunks, { type: "audio/wav" });

                let reader = new FileReader();
                reader.readAsDataURL(audioBlob);
                reader.onloadend = () => {
                    let base64Data = reader.result.split(',')[1]; // حذف قسمت data URL
                    resolve(base64Data); // بازگشت base64
                };

                audioChunks = [];
            };

            mediaRecorder.stop();
        } else {
            reject("ضبط فعال نیست.");
        }
    });
};

window.checkAudioDevices = async function () {
    try {
        let devices = await navigator.mediaDevices.enumerateDevices();
        let audioDevices = devices.filter(device => device.kind === "audioinput");

        if (audioDevices.length === 0) {
            console.error("🚨 هیچ میکروفونی پیدا نشد!");
            return false;
        }
        return true;
    } catch (error) {
        console.error("🚨 خطای دسترسی به دستگاه‌ها:", error);
        return false;
    }
};


