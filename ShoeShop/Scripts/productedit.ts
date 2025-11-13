(function () {
    interface IProductImage { id: string; path: string; alt: string; mode: ProductImageMode; }
    enum ProductImageMode { New = 1, Edit = 2, Original = 3, Deleted = 4 }

    const EDIT_PRODUCT_FORM = '[data-role="edit-product-form"]';
    const TEMPLATE_PRODUCT_IMAGE_SELECTOR = '[data-role="template-product-image"]';
    const IMAGES_PANEL_SELECTOR = '[data-role="images_panel"]';
    const ELEMENT_ROOT_SELECTOR = '[data-role="elemen-troot"]';
    const INPUT_IMAGE_ID_SELECTOR = '[data-role="input-image-id"]';
    const IMAGE_PRIVIEW_SELECTOR = '[data-role="image-priview"]';
    const INPUT_IMAGE_FILE_SELECTOR = '[data-role="input-image-file"]';
    const INPUT_IMAGE_ALT_SELECTOR = '[data-role="input-image-alt"]';
    const INPUT_IMAGE_MODE_SELECTOR = '[data-role="input-image-mode"]';
    const INPUT_IMAGE_FILE_ERROR_SELECTOR = '[data-role="input-image-file-error"]';
    const LABEL_INPUT_IMAGE_ALT_SELECTOR = '[data-role="label-input-image-alt"]';
    const LABEL_INPUT_IMAGE_FILE_SELECTOR = '[data-role="label-input-image-file"]';
    const BUTTON_DELETE_IMAGE_SELECTOR = '[data-role="button-delete-image"]';
    const BUTTON_CANCEL_IMAGE_SELECTOR = '[data-role="button-cansel-image"]';
    const BUTTON_SUBMIT_SELECTOR = '[data-role="button-submit"]';
    const BUTTON_ADD_IMAGE_SELECTOR = '[data-role="button-add-image"]';
    const OVERLAY_IMAGE_PANEL_SELECTOR = '[data-role="overlay-image-panel"]';

    const MAX_FILE_SIZE = 153600;
    const ALLOVED_FILE_TYPE = 'image/jpeg';

    const form: HTMLFormElement = document.querySelector(EDIT_PRODUCT_FORM);
    const imagesPanel: HTMLElement = form.querySelector(IMAGES_PANEL_SELECTOR);
    let imagePanelIndex: number = 0;

    const addImageButton = form.querySelector(BUTTON_ADD_IMAGE_SELECTOR);
    addImageButton.addEventListener('click', () => { addImagePanel(); });

    const submitButton = form.querySelector(BUTTON_SUBMIT_SELECTOR);
    submitButton.addEventListener('click', (event: Event) => {
        event.preventDefault();
        event.stopPropagation();

        let index: number = 0;
        const fileValidates: Array<boolean> = [];

        imagesPanel.querySelectorAll('[name]').forEach((element) => element.removeAttribute('name'));
        imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR).forEach((root: HTMLElement) => {

            const inputImageId = getInputImageId(root);
            const inputImageFile = getInputImageFile(root);
            const inputImageAlt = getInputImageAlt(root);
            const inputImageMode = getInputImageMode(root);

            const mode: ProductImageMode = ProductImageMode[inputImageMode.value];

            if (mode === ProductImageMode.New) {
                const isValidate = validateInputFile(inputImageFile);
                if (isValidate) {
                    inputImageId.setAttribute('name', `images[${index}].id`);
                    inputImageFile.setAttribute('name', `images[${index}].image`);
                    inputImageAlt.setAttribute('name', `images[${index}].alt`);
                    inputImageMode.setAttribute('name', `images[${index}].mode`);
                    index++;
                } else {
                    fileValidates.push(isValidate);
                    showErrorInputImageFile(root, true);
                }
            } else if (mode === ProductImageMode.Edit) {
                const isValidate = validateInputFile(inputImageFile);
                if (isValidate) {
                    inputImageId.setAttribute('name', `images[${index}].id`);
                    inputImageFile.setAttribute('name', `images[${index}].image`);
                    inputImageAlt.setAttribute('name', `images[${index}].alt`);
                    inputImageMode.setAttribute('name', `images[${index}].mode`);
                    index++;
                } else {
                    fileValidates.push(isValidate);
                    showErrorInputImageFile(root, true);
                }
            } else if (mode === ProductImageMode.Original) {
                inputImageId.setAttribute('name', `images[${index}].id`);
                inputImageAlt.setAttribute('name', `images[${index}].alt`);
                inputImageMode.setAttribute('name', `images[${index}].mode`);
                index++;
            } else if (mode === ProductImageMode.Deleted) {
                inputImageId.setAttribute('name', `images[${index}].id`);
                inputImageMode.setAttribute('name', `images[${index}].mode`);
                index++;
            }
        });

        if (form.checkValidity() && fileValidates.length === 0) {
            form.submit();
        } else {
            form.classList.add('was-validated');
        }
    });

    initialize();

    function initialize() {
        const productImageArray: NodeListOf<HTMLElement> = imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR);
        productImageArray.forEach((item) => {
            createImagePanel(item, imagesPanel, imagePanelIndex++);
        });
    }

    function addImagePanel() {
        const template: HTMLTemplateElement = document.querySelector(TEMPLATE_PRODUCT_IMAGE_SELECTOR);
        const rootNode: Node = template.content.cloneNode(true);
        imagesPanel.appendChild(rootNode);

        setTimeout(() => {
            const rootElements: NodeListOf<HTMLElement> = imagesPanel.querySelectorAll(ELEMENT_ROOT_SELECTOR);
            const rootElement = rootElements[rootElements.length - 1];

            createImagePanel(rootElement, imagesPanel, imagePanelIndex++);
        }, 100);
    }

    function createImagePanel(rootElement: HTMLElement, imagesPanel: HTMLElement, index: number) {
        const inputImageId = getInputImageId(rootElement);
        const imagePreview: HTMLImageElement = rootElement.querySelector(IMAGE_PRIVIEW_SELECTOR);
        const inputImageMode = getInputImageMode(rootElement);

        const buttonCancelImage = rootElement.querySelector(BUTTON_CANCEL_IMAGE_SELECTOR);
        const buttonDeleteImage = rootElement.querySelector(BUTTON_DELETE_IMAGE_SELECTOR);

        const inputImageAlt = getInputImageAlt(rootElement);
        inputImageAlt.id = `input-image-alt-${index}`;

        const labelInputImageAlt = rootElement.querySelector(LABEL_INPUT_IMAGE_ALT_SELECTOR);
        labelInputImageAlt.setAttribute('for', inputImageAlt.id);

        const inputImageFile = getInputImageFile(rootElement);
        inputImageFile.id = `input-image-file-${index}`;

        const labelInputImageFile = rootElement.querySelector(LABEL_INPUT_IMAGE_FILE_SELECTOR);
        labelInputImageFile.setAttribute('for', inputImageFile.id);

        const defaultData: IProductImage = {
            id: inputImageId.value, path: imagePreview.src, alt: inputImageAlt.value,
            mode: ProductImageMode[inputImageMode.value]
        };

        inputImageFile.addEventListener('change', () => {

            if (validateInputFile(inputImageFile)) {
                showErrorInputImageFile(rootElement, false);
            } else {
                showErrorInputImageFile(rootElement, true);
            }

            const file = inputImageFile.files[0];
            imagePreview.src = URL.createObjectURL(file);
            imagePreview.alt = imagePreview.title = file.name;

            const mode = ProductImageMode[inputImageMode.value];
            if (mode === ProductImageMode.Original) {
                inputImageMode.value = ProductImageMode[ProductImageMode.Edit];
            }
        });

        buttonCancelImage.addEventListener('click', () => {
            resetProductImage();
        });

        buttonDeleteImage.addEventListener('click', () => {
            removeProductImage();
        });

        function resetProductImage() {
            if (defaultData.mode === ProductImageMode.Original) {
                inputImageId.id = defaultData.id;
                inputImageAlt.value = defaultData.alt;
                inputImageMode.value = ProductImageMode[defaultData.mode];
                imagePreview.src = defaultData.path;

                showMessageDeleteImage(rootElement, false);
            } else if (defaultData.mode === ProductImageMode.New) {
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
            } else if (defaultData.mode === ProductImageMode.New) {
                rootElement.remove();
            }
        }
    }

    function validateInputFile(inputImageFile: HTMLInputElement) {
        if (inputImageFile.files.length !== 1) { return false; }
        const file = inputImageFile.files[0];
        if (file.size > MAX_FILE_SIZE) { return false; }
        if (file.type !== ALLOVED_FILE_TYPE) { return false; }

        return true;
    }

    function showErrorInputImageFile(root: HTMLElement, isShow: boolean) {
        const element: HTMLElement = root.querySelector(INPUT_IMAGE_FILE_ERROR_SELECTOR);
        if (isShow) {
            element.style.display = 'block';
        } else {
            element.style.display = 'none';
        }
    }

    function showMessageDeleteImage(root: HTMLElement, show: boolean) {
        const element = root.querySelector(OVERLAY_IMAGE_PANEL_SELECTOR);
        if (show) {
            element.classList.remove('d-none');
        } else {
            element.classList.add('d-none');
        }
    }

    function getInputImageId(root: HTMLElement): HTMLInputElement {
        return root.querySelector(INPUT_IMAGE_ID_SELECTOR);
    }

    function getInputImageFile(root: HTMLElement): HTMLInputElement {
        return root.querySelector(INPUT_IMAGE_FILE_SELECTOR);
    }

    function getInputImageMode(root: HTMLElement): HTMLInputElement {
        return root.querySelector(INPUT_IMAGE_MODE_SELECTOR);
    }

    function getInputImageAlt(root: HTMLElement): HTMLInputElement {
        return root.querySelector(INPUT_IMAGE_ALT_SELECTOR);
    }
}());
