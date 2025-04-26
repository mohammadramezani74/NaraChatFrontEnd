//window.playVideoFromApi = async (videoElementId, apiUrl) => {
//    try {
//        const response = await fetch(apiUrl);
//        if (!response.ok) {
//            console.error("Failed to fetch video:", response.statusText);
//            return;
//        }
//        const videoBlob = await response.blob();
//        const videoUrl = URL.createObjectURL(videoBlob);

//        const videoElement = document.getElementById(videoElementId);
//        if (videoElement) {
//            videoElement.src = videoUrl;
//            videoElement.play();
//        }
//    } catch (error) {
//        console.error("Error fetching video:", error);
//    }
//};
window.playVideoFromApi = async (videoElementId, apiUrl, token) => {
    try {
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "video/mp4"
            }
        });

        if (!response.ok) {
            console.error("Failed to fetch video:", response.statusText);
            return;
        }

        const videoBlob = await response.blob();
        const videoUrl = URL.createObjectURL(videoBlob);

        const videoElement = document.getElementById(`video-${videoElementId}`); // از شناسه ویدیو استفاده کنید
        if (videoElement) {
            videoElement.src = videoUrl;
            videoElement.play();
        }
    } catch (error) {
        console.error("Error fetching video:", error);
    }
};