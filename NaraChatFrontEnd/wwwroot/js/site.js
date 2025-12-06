function getScrollContainer(element) {
    const el = resolveElement(element);
    if (!el) return null;
    return el.closest?.("[data-scroll-container]") ?? el;
}

window.scrollToBottom = (element) => {

    const elem = document.getElementById("mainchat");
    if (!elem) return;

    const last = elem.lastElementChild;
    setTimeout(() => {
        if (last) {
            try {
                last.scrollIntoView({ behavior: "smooth", block: "end" });
            } catch {
                elem.scrollTop = elem.scrollHeight;
            }
        } else {
            elem.scrollTop = elem.scrollHeight;
        }
    }, 50);

  


  
  

};
function resolveElement(el) {
    if (!el) return null;

    if (el instanceof Element || el === window || el === document) return el;

    if (el.id) return document.getElementById(el.id);
    return null;
}
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
function triggerDownload2(fileData, fileName) {
    const link = document.createElement("a");
    link.href = fileData;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
function triggerDownload(base64Data, fileName, mimeType = "application/octet-stream") {
    const link = document.createElement("a");
    link.href = `data:${mimeType};base64,${base64Data}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

(function () {
    document.addEventListener('click', function (e) {
        const a = e.target.closest('a[data-mention-link]');
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
    const container = document.getElementById("mainchat");
    if (!container) return; 


    if (container._scrollHandler) {
        container.removeEventListener("scroll", container._scrollHandler);
        container._scrollHandler = null;
    }

    const handler = () => {
        if (container.scrollTop === 0) {
           if(element===1)
               dotnetHelper.invokeMethodAsync("LoadMoreMessages");
           else
               dotnetHelper.invokeMethodAsync("LoadMoreChannelMessages");
        }
    };
    container._scrollHandler = handler;
    container.addEventListener("scroll", handler, { passive: true });
};

window.disposeScrollListener = (element) => {
    const container = getScrollContainer(element);
    if (!container || !container._scrollHandler) return;
    container.removeEventListener("scroll", container._scrollHandler);
    container._scrollHandler = null;
};


window.getScrollPosition = (element) => {
    const container = getScrollContainer(element);
    return container ? container.scrollTop : 0;
};

window.setScrollPosition = (element, position) => {
    const container = getScrollContainer(element);
    if (container) {
        container.scrollTop = position;
    }
};

window.getScrollHeight = (element) => {
    const container = getScrollContainer(element);
    return container ? container.scrollHeight : 0;
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