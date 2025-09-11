 window.scrollToBottom = (element) => {
        element.scrollTop = element.scrollHeight;
};

function browserNotify(data) {
    if (!("Notification" in window)) {
        alert(data.title);
    }

    var option = {
        body: data.message,
        dir: "rtl",
        icon: data.avatar
    };

    if (Notification.permission == "granted") {
        var notification = new Notification(data.name, option);
        notification.onclick = function (event) {
            event.preventDefault;
            window.location.href = data.url;
            notification.close();
        }
    } else if (Notification.permission != "granted") {
        Notification.requestPermission().then(function (permission) {
            if (permission == "garnted") {
                var notification = new Notification(data.name, option);
                notification.onclick = function (event) {
                    event.preventDefault;
                    window.location.href = data.url;
                    notification.close();
                }
            }
        });
    }

}


window.scrollToElement = (id) => {
    const element = document.getElementById(id);
    if (element) {
        element.scrollIntoView({ behavior: "smooth", block: "center" });
    }
};


function triggerDownload(fileData, fileName) {
    const link = document.createElement("a");
    link.href = fileData;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

    (function () {
        document.addEventListener('click', function (e) {
            0922const a = e.target.closest('a[data-mention-link]');
            if (!a) return;
            e.preventDefault();
            const href = a.getAttribute('href');
            if (window.Blazor && typeof window.Blazor.navigateTo === 'function') {
                window.Blazor.navigateTo(href);
            } else {
                window.location.href = href;
            }
        }, true);
        })();


window.initScrollListener = (element, dotnetHelper) => {
    element.addEventListener("scroll", () => {
        if (element.scrollTop === 0) {
            dotnetHelper.invokeMethodAsync("LoadMoreMessages");
        }
    });
};

window.getScrollPosition = (element) => {
    return element ? element.scrollTop : 0;
};

window.setScrollPosition = (element, position) => {
    if (element) {
        element.scrollTop = position;
    }
};

window.getScrollHeight = (element) => {
    return element ? element.scrollHeight : 0;
};

window.getWindowSize = () => {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
    }).catch(function (err) {
        console.error('Could not copy text: ', err);
    });
}
