window.cropper = {
    instance: null,
    init: function () {
        var image = document.getElementById('image');
        this.instance = new Cropper(image, {
            viewMode: 1,          // حالت نمایش تصویر
            dragMode: 'move',     // حرکت تصویر
            aspectRatio: NaN,     // نسبت تصویر آزاد
            autoCrop: false,      // اجازه خودکار بریدن تصویر
            background: false,    // حذف پس‌زمینه شطرنجی
        
        });
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
