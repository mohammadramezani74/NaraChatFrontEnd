window.cropper = {
    instance: null,
    init: function () {
        if (this.instance) {
            this.instance.destroy();
        }
        var image = document.getElementById('image');
        if (!image) return; 
        var image = document.getElementById('image');
        this.instance = new Cropper(image, {
            viewMode: 0,          // حالت نمایش تصویر
            dragMode: 'move',     // حرکت تصویر
            aspectRatio: NaN,     // نسبت تصویر آزاد
            autoCrop: false,      // اجازه خودکار بریدن تصویر
            background: false,    // حذف پس‌زمینه شطرنجی
        
        });
    },
    destroy: function () {
        if (this.instance) {
            this.instance.destroy();
            this.instance = null;
        }
    },
    zoom: function (value) {
        this.instance.zoom(value);
    },
    rotate: function (degree) {
        this.instance.rotate(degree);
    },
    scaleX: function () {
        var scaleX = this.instance.getData().scaleX === 1 ? -1 : 1;
        this.instance.scaleX(scaleX);
    }
};
