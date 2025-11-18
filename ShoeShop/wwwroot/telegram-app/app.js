// Telegram Mini App JavaScript
let tg = window.Telegram.WebApp;
let currentPage = 'home';
let currentCategory = '';
let currentProduct = null;
let cart = JSON.parse(localStorage.getItem('cart') || '[]');
let products = [];
let currentFilters = {};
let currentSort = 'popular';

// Инициализация приложения
document.addEventListener('DOMContentLoaded', function() {
    initTelegramApp();
    loadProducts();
    updateCartBadge();
    showPage('home');
});

// Инициализация Telegram WebApp
function initTelegramApp() {
    tg.ready();
    tg.expand();
    
    // Применяем тему Telegram
    document.documentElement.style.setProperty('--tg-color-scheme', tg.colorScheme);
    
    // Настраиваем главную кнопку
    tg.MainButton.setText('Добавить в корзину');
    tg.MainButton.hide();
    
    // Обработчик главной кнопки
    tg.MainButton.onClick(function() {
        if (currentPage === 'product' && currentProduct) {
            addToCart(currentProduct);
        } else if (currentPage === 'cart') {
            checkout();
        }
    });
    
    // Обработчик кнопки назад
    tg.BackButton.onClick(function() {
        goBack();
    });
}

// Загрузка товаров с сервера
async function loadProducts() {
    try {
        const response = await fetch('/api/telegram/products');
        products = await response.json();
        
        displayPopularProducts();
        displayNewProducts();
    } catch (error) {
        console.error('Ошибка загрузки товаров:', error);
        tg.showAlert('Ошибка загрузки товаров');
    }
}

// Отображение популярных товаров
function displayPopularProducts() {
    const container = document.getElementById('popular-products');
    const popularProducts = products.slice(0, 4);
    
    container.innerHTML = popularProducts.map(product => `
        <div class="product-card" onclick="showProduct('${product.id}')">
            <img src="${product.image || '/images/no-image.jpg'}" alt="${product.name}" class="product-image">
            <div class="product-info">
                <div class="product-name">${product.name}</div>
                <div class="product-price">
                    ${product.salePrice ? `${product.salePrice} ₽ <span class="product-old-price">${product.price} ₽</span>` : `${product.price} ₽`}
                </div>
            </div>
        </div>
    `).join('');
}

// Отображение новинок
function displayNewProducts() {
    const container = document.getElementById('new-products');
    const newProducts = products.slice(-4);
    
    container.innerHTML = newProducts.map(product => `
        <div class="product-card" onclick="showProduct('${product.id}')">
            <img src="${product.image || '/images/no-image.jpg'}" alt="${product.name}" class="product-image">
            <div class="product-info">
                <div class="product-name">${product.name}</div>
                <div class="product-price">
                    ${product.salePrice ? `${product.salePrice} ₽ <span class="product-old-price">${product.price} ₽</span>` : `${product.price} ₽`}
                </div>
            </div>
        </div>
    `).join('');
}

// Навигация между страницами
function showPage(page) {
    // Скрываем все страницы
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    
    // Показываем нужную страницу
    document.getElementById(`${page}-page`).classList.add('active');
    document.querySelector(`.nav-item:nth-child(${getPageIndex(page)})`).classList.add('active');
    
    currentPage = page;
    
    // Управляем кнопками Telegram
    if (page === 'home') {
        tg.BackButton.hide();
        tg.MainButton.hide();
    } else if (page === 'cart') {
        tg.BackButton.show();
        if (cart.length > 0) {
            tg.MainButton.setText('Оформить заказ');
            tg.MainButton.show();
        } else {
            tg.MainButton.hide();
        }
        displayCart();
    } else if (page === 'profile') {
        tg.BackButton.show();
        tg.MainButton.hide();
        displayProfile();
    } else {
        tg.BackButton.show();
        tg.MainButton.hide();
    }
}

function getPageIndex(page) {
    const pages = ['home', 'catalog', 'cart', 'profile'];
    return pages.indexOf(page) + 1;
}

// Показать каталог
function showCatalog(category = '') {
    currentCategory = category;
    showPage('catalog');
    
    const title = document.getElementById('catalog-title');
    title.textContent = getCategoryName(category);
    
    displayCatalogProducts();
}

function getCategoryName(category) {
    const names = {
        'men': 'Мужские кроссовки',
        'women': 'Женские кроссовки', 
        'running': 'Беговые кроссовки',
        'sale': 'Распродажа',
        '': 'Все товары'
    };
    return names[category] || 'Каталог';
}

// Отображение товаров в каталоге
function displayCatalogProducts() {
    let filteredProducts = products;
    
    // Фильтрация по категории
    if (currentCategory) {
        filteredProducts = filteredProducts.filter(p => 
            p.category === currentCategory || 
            (currentCategory === 'sale' && p.salePrice)
        );
    }
    
    // Применение фильтров
    if (currentFilters.brand) {
        filteredProducts = filteredProducts.filter(p => 
            p.name.toLowerCase().includes(currentFilters.brand.toLowerCase())
        );
    }
    
    if (currentFilters.maxPrice) {
        filteredProducts = filteredProducts.filter(p => 
            (p.salePrice || p.price) <= currentFilters.maxPrice
        );
    }
    
    // Сортировка
    filteredProducts.sort((a, b) => {
        switch (currentSort) {
            case 'price-asc':
                return (a.salePrice || a.price) - (b.salePrice || b.price);
            case 'price-desc':
                return (b.salePrice || b.price) - (a.salePrice || a.price);
            case 'new':
                return new Date(b.dateAdded) - new Date(a.dateAdded);
            default:
                return 0;
        }
    });
    
    const container = document.getElementById('catalog-products');
    container.innerHTML = filteredProducts.map(product => `
        <div class="product-card" onclick="showProduct('${product.id}')">
            <img src="${product.image || '/images/no-image.jpg'}" alt="${product.name}" class="product-image">
            <div class="product-info">
                <div class="product-name">${product.name}</div>
                <div class="product-price">
                    ${product.salePrice ? `${product.salePrice} ₽ <span class="product-old-price">${product.price} ₽</span>` : `${product.price} ₽`}
                </div>
            </div>
        </div>
    `).join('');
}

// Показать товар
function showProduct(productId) {
    currentProduct = products.find(p => p.id === productId);
    if (!currentProduct) return;
    
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    document.getElementById('product-page').classList.add('active');
    currentPage = 'product';
    
    tg.BackButton.show();
    tg.MainButton.setText('Добавить в корзину');
    tg.MainButton.show();
    
    // Галерея
    const gallery = document.getElementById('product-gallery');
    gallery.innerHTML = `
        <img src="${currentProduct.image || '/images/no-image.jpg'}" alt="${currentProduct.name}" class="gallery-image">
    `;
    
    // Информация о товаре
    const info = document.getElementById('product-info');
    info.innerHTML = `
        <h1 class="product-title">${currentProduct.name}</h1>
        <div class="product-price-large">
            ${currentProduct.salePrice ? `${currentProduct.salePrice} ₽ <span class="product-old-price">${currentProduct.price} ₽</span>` : `${currentProduct.price} ₽`}
        </div>
    `;
    
    // Размеры
    const sizes = document.getElementById('product-sizes');
    sizes.innerHTML = `
        <h3>Выберите размер:</h3>
        <div class="sizes-grid">
            ${[40, 41, 42, 43, 44, 45].map(size => `
                <button class="size-btn" onclick="selectSize(${size})">${size}</button>
            `).join('')}
        </div>
    `;
    
    // Описание
    const description = document.getElementById('product-description');
    description.innerHTML = `
        <h3>Описание:</h3>
        <p>${currentProduct.description || currentProduct.content || 'Описание товара'}</p>
    `;
}

// Выбор размера
function selectSize(size) {
    document.querySelectorAll('.size-btn').forEach(btn => btn.classList.remove('selected'));
    event.target.classList.add('selected');
    currentProduct.selectedSize = size;
}

// Добавление в корзину
function addToCart(product) {
    if (!product.selectedSize) {
        tg.showAlert('Выберите размер');
        return;
    }
    
    const cartItem = {
        id: product.id,
        name: product.name,
        price: product.salePrice || product.price,
        size: product.selectedSize,
        image: product.image,
        quantity: 1
    };
    
    const existingItem = cart.find(item => 
        item.id === cartItem.id && item.size === cartItem.size
    );
    
    if (existingItem) {
        existingItem.quantity++;
    } else {
        cart.push(cartItem);
    }
    
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartBadge();
    
    tg.showAlert('Товар добавлен в корзину!');
}

// Отображение корзины
function displayCart() {
    const container = document.getElementById('cart-items');
    
    if (cart.length === 0) {
        container.innerHTML = '<div style="text-align: center; padding: 40px;">Корзина пуста</div>';
        document.getElementById('cart-total').textContent = '0 ₽';
        return;
    }
    
    container.innerHTML = cart.map((item, index) => `
        <div class="cart-item">
            <img src="${item.image || '/images/no-image.jpg'}" alt="${item.name}" class="cart-item-image">
            <div class="cart-item-info">
                <div class="cart-item-name">${item.name}</div>
                <div class="cart-item-size">Размер: ${item.size}</div>
                <div class="cart-item-controls">
                    <div class="quantity-controls">
                        <button class="quantity-btn" onclick="changeQuantity(${index}, -1)">-</button>
                        <span class="quantity">${item.quantity}</span>
                        <button class="quantity-btn" onclick="changeQuantity(${index}, 1)">+</button>
                    </div>
                    <div class="cart-item-price">${item.price * item.quantity} ₽</div>
                </div>
            </div>
        </div>
    `).join('');
    
    updateCartTotal();
}

// Изменение количества в корзине
function changeQuantity(index, delta) {
    cart[index].quantity += delta;
    
    if (cart[index].quantity <= 0) {
        cart.splice(index, 1);
    }
    
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartBadge();
    displayCart();
}

// Обновление общей суммы
function updateCartTotal() {
    const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    document.getElementById('cart-total').textContent = `${total} ₽`;
}

// Обновление бейджа корзины
function updateCartBadge() {
    const badge = document.getElementById('cart-badge');
    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    badge.textContent = totalItems;
    badge.style.display = totalItems > 0 ? 'block' : 'none';
}

// Оформление заказа
function checkout() {
    if (cart.length === 0) {
        tg.showAlert('Корзина пуста');
        return;
    }
    
    const total = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const orderData = {
        items: cart,
        total: total,
        user: tg.initDataUnsafe.user
    };
    
    // Отправляем заказ на сервер
    fetch('/api/telegram/order', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(orderData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            cart = [];
            localStorage.setItem('cart', JSON.stringify(cart));
            updateCartBadge();
            tg.showAlert(`Заказ оформлен! Номер: ${data.orderNumber}`);
            showPage('home');
        } else {
            tg.showAlert('Ошибка оформления заказа');
        }
    })
    .catch(error => {
        console.error('Ошибка:', error);
        tg.showAlert('Ошибка оформления заказа');
    });
}

// Отображение профиля
function displayProfile() {
    const container = document.getElementById('user-info');
    const user = tg.initDataUnsafe.user;
    
    if (user) {
        container.innerHTML = `
            <div class="user-name">${user.first_name} ${user.last_name || ''}</div>
        `;
    } else {
        container.innerHTML = `
            <div class="user-name">Пользователь Telegram</div>
        `;
    }
}

// Фильтры
function showFilters() {
    document.getElementById('filters-modal').classList.add('active');
}

function closeModal() {
    document.getElementById('filters-modal').classList.remove('active');
}

function applyFilters() {
    const brand = document.getElementById('brand-filter').value;
    const maxPrice = document.getElementById('price-range').value;
    
    currentFilters = {
        brand: brand,
        maxPrice: maxPrice ? parseInt(maxPrice) : null
    };
    
    displayCatalogProducts();
    closeModal();
}

function applySorting() {
    currentSort = document.getElementById('sort-select').value;
    displayCatalogProducts();
}

// Навигация
function goBack() {
    if (currentPage === 'product') {
        showPage('catalog');
    } else {
        showPage('home');
    }
}

// Обновление цены в фильтре
document.addEventListener('DOMContentLoaded', function() {
    const priceRange = document.getElementById('price-range');
    const priceValue = document.getElementById('price-value');
    
    if (priceRange && priceValue) {
        priceRange.addEventListener('input', function() {
            priceValue.textContent = `до ${this.value} ₽`;
        });
    }
});

// Применение промокода
function applyPromo() {
    const promoCode = document.getElementById('promo-input').value;
    if (promoCode) {
        tg.showAlert('Промокод применен!');
    }
}

// Избранное и заказы (заглушки)
function showOrders() {
    tg.showAlert('История заказов');
}

function showFavorites() {
    tg.showAlert('Избранные товары');
}

function toggleFavorite() {
    tg.showAlert('Добавлено в избранное');
}