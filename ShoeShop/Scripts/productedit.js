var ProductImageMode;
(function (ProductImageMode) {
    ProductImageMode[ProductImageMode["New"] = 1] = "New";
    ProductImageMode[ProductImageMode["Edit"] = 2] = "Edit";
    ProductImageMode[ProductImageMode["Original"] = 3] = "Original";
    ProductImageMode[ProductImageMode["Deleted"] = 4] = "Deleted";
})(ProductImageMode || (ProductImageMode = {}));
var ProductImage = /** @class */ (function () {
    function ProductImage() {
    }
    return ProductImage;
}());
(function () {
    var form = document.querySelector('[data-role="edit-product-form"]');
    initialize(form);
    function initialize(form) {
        var data = [];
        form.querySelector('[data-imagespahel="images_panel"]').querySelectorAll('[data-role="elemen-troot"]').forEach(function (item, index) {
        });
    }
}());
//# sourceMappingURL=productedit.js.map