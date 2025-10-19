window.naraChecklist = (function () {
    const instances = new Map();

    function register(containerId, handleId, dotNetRef) {
        const container = document.getElementById(containerId);
        const handle = document.getElementById(handleId);

        if (!container || !handle) {
            return;
        }

        dispose(containerId);

        let startX = 0;
        let startY = 0;
        let offsetX = 0;
        let offsetY = 0;
        let isDragging = false;

        const onPointerDown = (event) => {
            if (event.button !== 0) {
                return;
            }

            isDragging = true;
            const rect = container.getBoundingClientRect();
            startX = event.clientX;
            startY = event.clientY;
            offsetX = startX - rect.left;
            offsetY = startY - rect.top;
            container.style.cursor = "move";
            document.addEventListener("pointermove", onPointerMove);
            document.addEventListener("pointerup", onPointerUp);
        };

        const onPointerMove = (event) => {
            if (!isDragging) {
                return;
            }

            const left = event.clientX - offsetX;
            const top = event.clientY - offsetY;
            container.style.left = `${Math.max(left, 0)}px`;
            container.style.top = `${Math.max(top, 0)}px`;
        };

        const onPointerUp = (event) => {
            if (!isDragging) {
                return;
            }

            isDragging = false;
            container.style.cursor = "";
            document.removeEventListener("pointermove", onPointerMove);
            document.removeEventListener("pointerup", onPointerUp);

            if (dotNetRef) {
                const rect = container.getBoundingClientRect();
                dotNetRef.invokeMethodAsync("OnDragFinished", rect.left, rect.top);
            }
        };

        handle.addEventListener("pointerdown", onPointerDown);

        instances.set(containerId, {
            cleanup: () => {
                handle.removeEventListener("pointerdown", onPointerDown);
                document.removeEventListener("pointermove", onPointerMove);
                document.removeEventListener("pointerup", onPointerUp);
            }
        });
    }

    function updatePosition(containerId, left, top) {
        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }
        container.style.left = `${left}px`;
        container.style.top = `${top}px`;
    }

    function dispose(containerId) {
        const existing = instances.get(containerId);
        if (existing) {
            existing.cleanup();
            instances.delete(containerId);
        }
    }

    return {
        register,
        updatePosition,
        dispose
    };
})();