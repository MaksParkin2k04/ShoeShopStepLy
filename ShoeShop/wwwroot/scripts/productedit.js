(function () {
    var ProductImageMode;
    (function (ProductImageMode) {
        ProductImageMode[ProductImageMode["New"] = 1] = "New";
        ProductImageMode[ProductImageMode["Edit"] = 2] = "Edit";
        ProductImageMode[ProductImageMode["Original"] = 3] = "Original";
        ProductImageMode[ProductImageMode["Deleted"] = 4] = "Deleted";
    })(ProductImageMode || (ProductImageMode = {}));
    var EDIT_PRODUCT_FORM = '[data-role="edit-product-form"]';
    var TEMPLATE_PRODUCT_IMAGE_SELECTOR = '[data-role="template-product-image"]';
    var IMAGES_PANEL_SELECTOR = '[data-role="images_panel"]';
    var ELEMENT_ROOT_SELECTOR = '[data-role="elemen-troot"]';
    var INPUT_IMAGE_ID_SELECTOR = '[data-role="input-image-id"]';
    var IMAGE_PRIVIEW_SELECTOR = '[data-role="image-priview"]';
    var INPUT_IMAGE_FILE_SELECTOR = '[data-role="input-image-file"]';
    var INPUT_IMAGE_ALT_SELECTOR = '[data-role="input-image-alt"]';
    var INPUT_IMAGE_MODE_SELECTOR = '[data-role="input-image-mode"]';
    var INPUT_IMAGE_FILE_ERROR_SELECTOR = '[data-role="input-image-file-error"]';
    var LABEL_INPUT_IMAGE_ALT_SELECTOR = '[data-role="label-input-image-alt"]';
    var LABEL_INPUT_IMAGE_FILE_SELECTOR = '[data-role="label-input-image-file"]';
    var BUTTON_DELETE_IMAGE_SELECTOR = '[data-role="button-delete-image"]';
    var BUTTON_CANCEL_IMAGE_SELECTOR = '[data-role="button-cansel-image"]';
    var BUTTON_SUBMIT_SELECTOR = '[data-role="button-submit"]';
    var BUTTON_ADD_IMAGE_SELECTOR = '[data-role="button-add-image"]';
    var OVERLAY_IMAGE_PANEL_SELECTOR = '[data-role="overlay-image-panel"]';
    var MAX_FILE_SIZE = 153600;
    var ALLOVED_FILE_TYPE = 'image/jpeg';
    var form = document.querySelector(EDIT_PRODUCT_FORM);
    var imagesPanel = form.querySelector(IMAGES_PANEL_SELECTOR);
    var imagePanelIndex = 0;
    var addImageButton = form.querySelector(BUTTON_ADD_IMAGE_SELECTOR);
    addImageButton.addEventListener('click', function () { addImagePanel(); });
    var submitButton = form.querySelector(BUTTON_SUBMIT_SELECTOR);
    submitButton.addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        var index = 0;
        var fileValidates = [];
        imagesPanel.querySelectorAll('[name]').forEach(function (element) { return element.removeAttribute('name'); });
        imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR).forEach(function (root) {
            var inputImageId = getInputImageId(root);
            var inputImageFile = getInputImageFile(root);
            var inputImageAlt = getInputImageAlt(root);
            var inputImageMode = getInputImageMode(root);
            var mode = ProductImageMode[inputImageMode.value];
            if (mode === ProductImageMode.New) {
                var isValidate = validateInputFile(inputImageFile);
                if (isValidate) {
                    inputImageId.setAttribute('name', "images[".concat(index, "].id"));
                    inputImageFile.setAttribute('name', "images[".concat(index, "].image"));
                    inputImageAlt.setAttribute('name', "images[".concat(index, "].alt"));
                    inputImageMode.setAttribute('name', "images[".concat(index, "].mode"));
                    index++;
                }
                else {
                    fileValidates.push(isValidate);
                    showErrorInputImageFile(root, true);
                }
            }
            else if (mode === ProductImageMode.Edit) {
                var isValidate = validateInputFile(inputImageFile);
                if (isValidate) {
                    inputImageId.setAttribute('name', "images[".concat(index, "].id"));
                    inputImageFile.setAttribute('name', "images[".concat(index, "].image"));
                    inputImageAlt.setAttribute('name', "images[".concat(index, "].alt"));
                    inputImageMode.setAttribute('name', "images[".concat(index, "].mode"));
                    index++;
                }
                else {
                    fileValidates.push(isValidate);
                    showErrorInputImageFile(root, true);
                }
            }
            else if (mode === ProductImageMode.Original) {
                inputImageId.setAttribute('name', "images[".concat(index, "].id"));
                inputImageAlt.setAttribute('name', "images[".concat(index, "].alt"));
                inputImageMode.setAttribute('name', "images[".concat(index, "].mode"));
                index++;
            }
            else if (mode === ProductImageMode.Deleted) {
                inputImageId.setAttribute('name', "images[".concat(index, "].id"));
                inputImageMode.setAttribute('name', "images[".concat(index, "].mode"));
                index++;
            }
        });
        if (form.checkValidity() && fileValidates.length === 0) {
            form.submit();
        }
        else {
            form.classList.add('was-validated');
        }
    });
    initialize();
    function initialize() {
        var productImageArray = imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR);
        productImageArray.forEach(function (item) {
            createImagePanel(item, imagesPanel, imagePanelIndex++);
        });
    }
    function addImagePanel() {
        var template = document.querySelector(TEMPLATE_PRODUCT_IMAGE_SELECTOR);
        var rootNode = template.content.cloneNode(true);
        imagesPanel.appendChild(rootNode);
        setTimeout(function () {
            var rootElements = imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR);
            var rootElement = rootElements[rootElements.length - 1];
            createImagePanel(rootElement, imagesPanel, imagePanelIndex++);
        }, 100);
    }
    function createImagePanel(rootElement, imagesPanel, index) {
        var inputImageId = getInputImageId(rootElement);
        var imagePreview = rootElement.querySelector(IMAGE_PRIVIEW_SELECTOR);
        var inputImageMode = getInputImageMode(rootElement);
        var buttonCancelImage = rootElement.querySelector(BUTTON_CANCEL_IMAGE_SELECTOR);
        var buttonDeleteImage = rootElement.querySelector(BUTTON_DELETE_IMAGE_SELECTOR);
        var inputImageAlt = getInputImageAlt(rootElement);
        inputImageAlt.id = "input-image-alt-".concat(index);
        var labelInputImageAlt = rootElement.querySelector(LABEL_INPUT_IMAGE_ALT_SELECTOR);
        labelInputImageAlt.setAttribute('for', inputImageAlt.id);
        var inputImageFile = getInputImageFile(rootElement);
        inputImageFile.id = "input-image-file-".concat(index);
        var labelInputImageFile = rootElement.querySelector(LABEL_INPUT_IMAGE_FILE_SELECTOR);
        labelInputImageFile.setAttribute('for', inputImageFile.id);
        var defaultData = {
            id: inputImageId.value, path: imagePreview.src, alt: inputImageAlt.value,
            mode: ProductImageMode[inputImageMode.value]
        };
        inputImageFile.addEventListener('change', function () {
            if (validateInputFile(inputImageFile)) {
                showErrorInputImageFile(rootElement, false);
            }
            else {
                showErrorInputImageFile(rootElement, true);
            }
            var file = inputImageFile.files[0];
            imagePreview.src = URL.createObjectURL(file);
            imagePreview.alt = imagePreview.title = file.name;
            var mode = ProductImageMode[inputImageMode.value];
            if (mode === ProductImageMode.Original) {
                inputImageMode.value = ProductImageMode[ProductImageMode.Edit];
            }
        });
        buttonCancelImage.addEventListener('click', function () {
            resetProductImage();
        });
        buttonDeleteImage.addEventListener('click', function () {
            removeProductImage();
        });
        function resetProductImage() {
            if (defaultData.mode === ProductImageMode.Original) {
                inputImageId.id = defaultData.id;
                inputImageAlt.value = defaultData.alt;
                inputImageMode.value = ProductImageMode[defaultData.mode];
                imagePreview.src = defaultData.path;
                showMessageDeleteImage(rootElement, false);
            }
            else if (defaultData.mode === ProductImageMode.New) {
                removeProductImage();
            }
        }
        function removeProductImage() {
            if (defaultData.mode === ProductImageMode.Original) {
                inputImageId.id = defaultData.id;
                inputImageAlt.value = defaultData.alt;
                inputImageMode.value = ProductImageMode[ProductImageMode.Deleted];
                imagePreview.src = defaultData.path;
                showMessageDeleteImage(rootElement, true);
            }
            else if (defaultData.mode === ProductImageMode.New) {
                rootElement.remove();
            }
        }
    }
    function validateInputFile(inputImageFile) {
        if (inputImageFile.files.length !== 1) {
            return false;
        }
        var file = inputImageFile.files[0];
        if (file.size > MAX_FILE_SIZE) {
            return false;
        }
        if (file.type !== ALLOVED_FILE_TYPE) {
            return false;
        }
        return true;
    }
    function showErrorInputImageFile(root, isShow) {
        var element = root.querySelector(INPUT_IMAGE_FILE_ERROR_SELECTOR);
        if (isShow) {
            element.style.display = 'block';
        }
        else {
            element.style.display = 'none';
        }
    }
    function showMessageDeleteImage(root, show) {
        var element = root.querySelector(OVERLAY_IMAGE_PANEL_SELECTOR);
        if (show) {
            element.classList.remove('d-none');
        }
        else {
            element.classList.add('d-none');
        }
    }
    function getInputImageId(root) {
        return root.querySelector(INPUT_IMAGE_ID_SELECTOR);
    }
    function getInputImageFile(root) {
        return root.querySelector(INPUT_IMAGE_FILE_SELECTOR);
    }
    function getInputImageMode(root) {
        return root.querySelector(INPUT_IMAGE_MODE_SELECTOR);
    }
    function getInputImageAlt(root) {
        return root.querySelector(INPUT_IMAGE_ALT_SELECTOR);
    }
}());
//# sourceMappingURL=productedit.js.map