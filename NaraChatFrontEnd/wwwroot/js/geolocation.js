window.getLiveLocation = function () {
    if (navigator.geolocation) {
        navigator.geolocation.watchPosition(function (position) {
            const coords = {
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            };
            console.log(coords);
            DotNet.invokeMethodAsync('NaraChatFrontEnd', 'ReceiveLocationFromJS', coords);
        });
    } else {
        alert("مرورگر شما از Geolocation پشتیبانی نمی‌کند.");
    }
};

window.startLocationWatcher = function (dotnetHelper) {
    console.log("startLocationWatcher called");

    navigator.geolocation.getCurrentPosition(function (position) {
        console.log("Got position:", position);

        dotnetHelper.invokeMethodAsync('ReceiveLocationFromJS', {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
        });
    }, function (error) {
        console.error("Error getting location:", error);
    });
};
