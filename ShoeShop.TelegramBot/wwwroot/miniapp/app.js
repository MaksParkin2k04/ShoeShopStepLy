// Telegram Mini App JavaScript
class TelegramShop {
    constructor() {
        this.tg = window.Telegram.WebApp;
        this.apiUrl = 'https://jxpc5n7p-7002.euw.devtunnels.ms/api';
        this.cart = JSON.parse(localStorage.getItem('cart') || '[]');
        this.products = [];
        this.currentProduct = null;
        this.selectedSize = null;
        
        this.init();
    }
    
    init() {
        // Инициализация Telegram WebApp
        this.tg.ready();
        this.tg.expand();
        
        // Применяем тему Telegram
        this.applyTelegramTheme();
        
        // Загружаем товары
        this.loadProducts();
        
        // Обновляем счетчик корзины
        this.updateCartCount();
        
        // Настраиваем главную кнопку
        this.setupMainButton();
        
        console.log('🤖 Telegram Shop инициализирован');
    }
    
    applyTelegramTheme() {
        if (this.tg.colorScheme === 'dark') {
            document.documentElement.style.setProperty('--tg-theme-bg-color', '#1c1c1e');
            document.documentElement.style.setProperty('--tg-theme-text-color', '#ffffff');
            document.documentElement.style.setProperty('--tg-theme-secondary-bg-color', '#2c2c2e');
        }
    }
    
    setupMainButton() {
        this.tg.MainButton.setText('Главная');
        this.tg.MainButton.onClick(() => this.showHome());
    }
    
    async loadProducts() {
        try {
            this.showLoading(true);
            const response = await fetch(`${this.apiUrl}/products`);
            this.products = await response.json();
            console.log('📦 Загружено товаров:', this.products.length);
        } catch (error) {
            console.error('❌ Ошибка загрузки товаров:', error);
            this.tg.showAlert('Ошибка загрузки товаров');
        } finally {
            this.showLoading(false);
        }
    }
    
    showLoading(show) {
        const loading = document.getElementById('loading');
        loading.classList.toggle('hidden', !show);
    }
    
    showPage(pageId) {
        document.querySelectorAll('.page').forEach(page => {
            page.classList.remove('active');
        });
        document.getElementById(pageId).classList.add('active');
        
        // Управление кнопками Telegram
        if (pageId === 'home-page') {
            this.tg.BackButton.hide();
            this.tg.MainButton.hide();
        } else {
            this.tg.BackButton.show();
            this.tg.MainButton.hide();
        }
    }
    
    showHome() {
        this.showPage('home-page');
        this.tg.BackButton.offClick(this.goBack);
    }
    
    showCatalog(category = null) {
        this.showPage('catalog-page');
        this.renderProducts(category);
        
        this.tg.BackButton.onClick(() => this.showHome());
    }
    
    renderProducts(category = null) {
        const grid = document.getElementById('products-grid');
        let filteredProducts = this.products;
        
        if (category) {
            filteredProducts = this.products.filter(p => 
                p.category.toLowerCase().includes(category.toLowerCase())
            );
        }
        
        if (filteredProducts.length === 0) {
            grid.innerHTML = `
                <div class="empty-cart">
                    <div class="empty-cart-icon">📦</div>
                    <p>Товары не найдены</p>
                </div>
            `;
            return;
        }
        
        grid.innerHTML = filteredProducts.map(product => `
            <div class="product-card" onclick="shop.showProduct('${product.id}')">
                <div class="product-image">👟</div>
                <div class="product-info">
                    <div class="product-name">${product.name}</div>
                    <div class="product-price ${product.salePrice ? 'sale' : ''}">
                        ${product.finalPrice.toLocaleString()} ₽
                        ${product.salePrice ? `<span class="old-price">${product.price.toLocaleString()} ₽</span>` : ''}
                    </div>
                </div>
            </div>
        `).join('');
    }
    
    showProduct(productId) {
        this.currentProduct = this.products.find(p => p.id === productId);
        if (!this.currentProduct) return;
        
        this.showPage('product-page');
        this.renderProductDetail();
        
        this.tg.BackButton.onClick(() => this.showCatalog());
    }
    
    renderProductDetail() {
        const container = document.getElementById('product-detail');
        const product = this.currentProduct;
        
        container.innerHTML = `
            <div class="product-detail">
                <div class="product-detail-image">👟</div>
                <div class="product-detail-info">
                    <h3>${product.name}</h3>
                    <div class="product-detail-price">
                        ${product.finalPrice.toLocaleString()} ₽
                        ${product.salePrice ? `<span class="old-price">${product.price.toLocaleString()} ₽</span>` : ''}
                    </div>
                    <div class="product-description">${product.content}</div>
                    
                    <div class="sizes">
                        <h4>Выберите размер:</h4>
                        <div class="size-grid">
                            ${product.sizes.map(size => `
                                <div class="size-btn" onclick="shop.selectSize(${size})">${size}</div>
                            `).join('')}
                        </div>
                    </div>
                    
                    <button class="btn-primary btn-full" onclick="shop.addToCart()" id="add-to-cart-btn" disabled>
                        Добавить в корзину
                    </button>
                </div>
            </div>
        `;
    }
    
    selectSize(size) {
        this.selectedSize = size;
        
        // Обновляем UI
        document.querySelectorAll('.size-btn').forEach(btn => {
            btn.classList.remove('selected');
        });
        event.target.classList.add('selected');
        
        // Активируем кнопку
        document.getElementById('add-to-cart-btn').disabled = false;
    }
    
    addToCart() {
        if (!this.selectedSize) {
            this.tg.showAlert('Выберите размер');
            return;
        }
        
        const cartItem = {
            id: this.currentProduct.id,
            name: this.currentProduct.name,
            price: this.currentProduct.finalPrice,
            size: this.selectedSize,
            quantity: 1,
            image: '👟'
        };
        
        // Проверяем, есть ли уже такой товар
        const existingItem = this.cart.find(item => 
            item.id === cartItem.id && item.size === cartItem.size
        );
        
        if (existingItem) {
            existingItem.quantity++;
        } else {
            this.cart.push(cartItem);
        }
        
        this.saveCart();
        this.updateCartCount();
        
        this.tg.showAlert('Товар добавлен в корзину!');
        this.selectedSize = null;
        document.getElementById('add-to-cart-btn').disabled = true;
        document.querySelectorAll('.size-btn').forEach(btn => {
            btn.classList.remove('selected');
        });
    }
    
    showCart() {
        this.showPage('cart-page');
        this.renderCart();
        
        this.tg.BackButton.onClick(() => this.goBack());
    }
    
    renderCart() {
        const container = document.getElementById('cart-items');
        
        if (this.cart.length === 0) {
            container.innerHTML = `
                <div class="empty-cart">
                    <div class="empty-cart-icon">🛒</div>
                    <p>Корзина пуста</p>
                    <button class="btn-primary" onclick="shop.showCatalog()">Перейти к покупкам</button>
                </div>
            `;
            document.querySelector('.cart-footer').style.display = 'none';
            return;
        }
        
        document.querySelector('.cart-footer').style.display = 'block';
        
        container.innerHTML = this.cart.map((item, index) => `
            <div class="cart-item">
                <div class="cart-item-image">${item.image}</div>
                <div class="cart-item-info">
                    <div class="cart-item-name">${item.name}</div>
                    <div class="cart-item-size">Размер: ${item.size}</div>
                    <div class="cart-item-price">${item.price.toLocaleString()} ₽</div>
                </div>
                <div class="cart-item-controls">
                    <button class="quantity-btn" onclick="shop.changeQuantity(${index}, -1)">-</button>
                    <span>${item.quantity}</span>
                    <button class="quantity-btn" onclick="shop.changeQuantity(${index}, 1)">+</button>
                </div>
            </div>
        `).join('');
        
        this.updateCartTotal();
    }
    
    changeQuantity(index, delta) {
        this.cart[index].quantity += delta;
        
        if (this.cart[index].quantity <= 0) {
            this.cart.splice(index, 1);
        }
        
        this.saveCart();
        this.updateCartCount();
        this.renderCart();
    }
    
    updateCartCount() {
        const count = this.cart.reduce((sum, item) => sum + item.quantity, 0);
        document.getElementById('cart-count').textContent = count;
        document.getElementById('cart-count-2').textContent = count;
    }
    
    updateCartTotal() {
        const total = this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        document.getElementById('cart-total').textContent = `${total.toLocaleString()} ₽`;
    }
    
    checkout() {
        if (this.cart.length === 0) {
            this.tg.showAlert('Корзина пуста');
            return;
        }
        
        this.showPage('checkout-page');
        this.renderCheckout();
        
        this.tg.BackButton.onClick(() => this.showCart());
    }
    
    renderCheckout() {
        // Заполняем данные пользователя из Telegram
        if (this.tg.initDataUnsafe.user) {
            const user = this.tg.initDataUnsafe.user;
            document.getElementById('customer-name').value = 
                `${user.first_name || ''} ${user.last_name || ''}`.trim();
        }
        
        // Отображаем товары в заказе
        const container = document.getElementById('order-items');
        container.innerHTML = this.cart.map(item => `
            <div class="order-item">
                <span>${item.name} (${item.size}) × ${item.quantity}</span>
                <span>${(item.price * item.quantity).toLocaleString()} ₽</span>
            </div>
        `).join('');
        
        const total = this.cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        document.getElementById('order-total').textContent = `${total.toLocaleString()} ₽`;
        
        // Обработчик формы
        document.getElementById('checkout-form').onsubmit = (e) => {
            e.preventDefault();
            this.submitOrder();
        };
    }
    
    async submitOrder() {
        const name = document.getElementById('customer-name').value;
        const phone = document.getElementById('customer-phone').value;
        const address = document.getElementById('customer-address').value;
        
        if (!name || !phone || !address) {
            this.tg.showAlert('Заполните все поля');
            return;
        }
        
        const orderData = {
            items: this.cart.map(item => ({
                productId: item.id,
                name: item.name,
                price: item.price,
                size: item.size,
                quantity: item.quantity
            })),
            customer: {
                name: name,
                phone: phone,
                address: address
            },
            source: 'Telegram Mini App',
            telegramUserId: this.tg.initDataUnsafe.user?.id
        };
        
        try {
            this.showLoading(true);
            
            const response = await fetch(`${this.apiUrl}/orders`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(orderData)
            });
            
            if (response.ok) {
                const order = await response.json();
                this.showSuccess(order.orderNumber);
                this.cart = [];
                this.saveCart();
                this.updateCartCount();
            } else {
                throw new Error('Ошибка создания заказа');
            }
        } catch (error) {
            console.error('❌ Ошибка заказа:', error);
            this.tg.showAlert('Ошибка оформления заказа');
        } finally {
            this.showLoading(false);
        }
    }
    
    showSuccess(orderNumber) {
        this.showPage('success-page');
        document.getElementById('order-number').textContent = `Номер заказа: ${orderNumber}`;
        
        this.tg.BackButton.hide();
    }
    
    sortProducts() {
        const sortBy = document.getElementById('sort-select').value;
        
        switch (sortBy) {
            case 'name':
                this.products.sort((a, b) => a.name.localeCompare(b.name));
                break;
            case 'price-asc':
                this.products.sort((a, b) => a.finalPrice - b.finalPrice);
                break;
            case 'price-desc':
                this.products.sort((a, b) => b.finalPrice - a.finalPrice);
                break;
        }
        
        this.renderProducts();
    }
    
    goBack() {
        // Логика возврата на предыдущую страницу
        const activePage = document.querySelector('.page.active').id;
        
        switch (activePage) {
            case 'catalog-page':
                this.showHome();
                break;
            case 'product-page':
                this.showCatalog();
                break;
            case 'cart-page':
                this.showCatalog();
                break;
            case 'checkout-page':
                this.showCart();
                break;
            default:
                this.showHome();
        }
    }
    
    saveCart() {
        localStorage.setItem('cart', JSON.stringify(this.cart));
    }
}

// Глобальные функции для HTML
function showHome() { shop.showHome(); }
function showCatalog(category) { shop.showCatalog(category); }
function showCart() { shop.showCart(); }
function checkout() { shop.checkout(); }
function goBack() { shop.goBack(); }
function sortProducts() { shop.sortProducts(); }

// Инициализация приложения
let shop;
document.addEventListener('DOMContentLoaded', () => {
    shop = new TelegramShop();
});

console.log('🚀 Telegram Mini App загружен');