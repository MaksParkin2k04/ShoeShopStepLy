class SupportChat {
    constructor() {
        this.isOpen = false;
        this.userId = this.generateUserId();
        this.userName = 'Гость';
        this.isTyping = false;
        this.init();
    }

    generateUserId() {
        return 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    init() {
        this.createChatWidget();
        this.bindEvents();
        this.loadChatHistory();
    }

    createChatWidget() {
        const chatHTML = `
            <div id="support-chat" class="support-chat-widget">
                <div class="chat-toggle" onclick="supportChat.toggle()">
                    <div class="chat-icon">
                        <i class="fas fa-comments"></i>
                        <span class="notification-badge" id="chat-badge" style="display: none;">0</span>
                    </div>
                    <div class="chat-text">
                        <div class="chat-title">Поддержка</div>
                        <div class="chat-subtitle">Мы онлайн</div>
                    </div>
                </div>
                
                <div class="chat-window" id="chat-window" style="display: none;">
                    <div class="chat-header">
                        <div class="chat-header-info">
                            <div class="agent-avatar">
                                <i class="fas fa-user-headset"></i>
                            </div>
                            <div class="agent-info">
                                <div class="agent-name">Служба поддержки</div>
                                <div class="agent-status">
                                    <span class="status-dot online"></span>
                                    В сети
                                </div>
                            </div>
                        </div>
                        <button class="chat-close" onclick="supportChat.close()">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    
                    <div class="chat-messages" id="chat-messages">
                        <div class="welcome-message">
                            <div class="bot-message">
                                <div class="message-avatar">
                                    <i class="fas fa-robot"></i>
                                </div>
                                <div class="message-content">
                                    <div class="message-text">
                                        👋 Добро пожаловать в службу поддержки StepLy!<br>
                                        Я виртуальный помощник. Могу ответить на вопросы о товарах, доставке и оплате.
                                    </div>
                                    <div class="message-time">${new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute: '2-digit'})}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="chat-input-area">
                        <div class="quick-actions" id="quick-actions">
                            <button class="quick-btn" onclick="supportChat.sendQuickMessage('Расскажите о доставке')">
                                🚚 Доставка
                            </button>
                            <button class="quick-btn" onclick="supportChat.sendQuickMessage('Как оплатить заказ?')">
                                💳 Оплата
                            </button>
                            <button class="quick-btn" onclick="supportChat.sendQuickMessage('Таблица размеров')">
                                📏 Размеры
                            </button>
                        </div>
                        <div class="chat-input">
                            <input type="text" id="chat-input" placeholder="Напишите сообщение..." maxlength="500">
                            <button id="send-btn" onclick="supportChat.sendMessage()">
                                <i class="fas fa-paper-plane"></i>
                            </button>
                        </div>
                        <div class="chat-footer">
                            <small class="text-muted">Обычно отвечаем в течение нескольких минут</small>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', chatHTML);
    }

    bindEvents() {
        const input = document.getElementById('chat-input');
        const sendBtn = document.getElementById('send-btn');
        
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        input.addEventListener('input', () => {
            sendBtn.classList.toggle('active', input.value.trim().length > 0);
        });

        // Автоматическое обновление чата
        setInterval(() => {
            this.checkForNewMessages();
        }, 3000);
    }

    toggle() {
        this.isOpen = !this.isOpen;
        const chatWindow = document.getElementById('chat-window');
        const chatToggle = document.querySelector('.chat-toggle');
        
        if (this.isOpen) {
            chatWindow.style.display = 'flex';
            chatToggle.classList.add('active');
            this.scrollToBottom();
            document.getElementById('chat-input').focus();
            this.hideNotificationBadge();
        } else {
            chatWindow.style.display = 'none';
            chatToggle.classList.remove('active');
        }
    }

    close() {
        this.isOpen = false;
        document.getElementById('chat-window').style.display = 'none';
        document.querySelector('.chat-toggle').classList.remove('active');
    }

    async sendMessage() {
        const input = document.getElementById('chat-input');
        const message = input.value.trim();
        
        if (!message) return;

        this.addUserMessage(message);
        input.value = '';
        document.getElementById('send-btn').classList.remove('active');
        
        this.hideQuickActions();
        this.showTypingIndicator();

        try {
            const response = await fetch('/api/chat/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    userId: this.userId,
                    userName: this.userName,
                    message: message
                })
            });

            const data = await response.json();
            
            setTimeout(() => {
                this.hideTypingIndicator();
                if (data.success && data.response) {
                    this.addBotMessage(data.response);
                } else {
                    this.addBotMessage('Извините, произошла ошибка. Попробуйте еще раз.');
                }
            }, 1000);

        } catch (error) {
            this.hideTypingIndicator();
            this.addBotMessage('Ошибка соединения. Проверьте интернет-подключение.');
        }
    }

    sendQuickMessage(message) {
        document.getElementById('chat-input').value = message;
        this.sendMessage();
    }

    addUserMessage(message) {
        const messagesContainer = document.getElementById('chat-messages');
        const messageHTML = `
            <div class="user-message">
                <div class="message-content">
                    <div class="message-text">${this.escapeHtml(message)}</div>
                    <div class="message-time">${new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute: '2-digit'})}</div>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }

    addBotMessage(message) {
        const messagesContainer = document.getElementById('chat-messages');
        const messageHTML = `
            <div class="bot-message">
                <div class="message-avatar">
                    <i class="fas fa-robot"></i>
                </div>
                <div class="message-content">
                    <div class="message-text">${this.formatBotMessage(message)}</div>
                    <div class="message-time">${new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute: '2-digit'})}</div>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }

    showTypingIndicator() {
        if (document.getElementById('typing-indicator')) return;
        
        const messagesContainer = document.getElementById('chat-messages');
        const typingHTML = `
            <div class="bot-message typing-indicator" id="typing-indicator">
                <div class="message-avatar">
                    <i class="fas fa-robot"></i>
                </div>
                <div class="message-content">
                    <div class="typing-dots">
                        <span></span>
                        <span></span>
                        <span></span>
                    </div>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', typingHTML);
        this.scrollToBottom();
    }

    hideTypingIndicator() {
        const indicator = document.getElementById('typing-indicator');
        if (indicator) {
            indicator.remove();
        }
    }

    hideQuickActions() {
        const quickActions = document.getElementById('quick-actions');
        if (quickActions && quickActions.children.length > 0) {
            quickActions.style.display = 'none';
        }
    }

    showNotificationBadge(count) {
        const badge = document.getElementById('chat-badge');
        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'block';
        }
    }

    hideNotificationBadge() {
        document.getElementById('chat-badge').style.display = 'none';
    }

    scrollToBottom() {
        const messagesContainer = document.getElementById('chat-messages');
        setTimeout(() => {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }, 100);
    }

    formatBotMessage(message) {
        return message
            .replace(/\n/g, '<br>')
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>');
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    async loadChatHistory() {
        try {
            const response = await fetch(`/api/chat/history/${this.userId}`);
            const messages = await response.json();
            
            // Загружаем только последние сообщения если есть история
            if (messages && messages.length > 0) {
                const messagesContainer = document.getElementById('chat-messages');
                messagesContainer.innerHTML = ''; // Очищаем приветствие
                
                messages.forEach(msg => {
                    this.addUserMessage(msg.message);
                    if (msg.response) {
                        this.addBotMessage(msg.response);
                    }
                });
            }
        } catch (error) {
            console.log('Не удалось загрузить историю чата');
        }
    }

    async checkForNewMessages() {
        try {
            const response = await fetch(`/api/chat/history/${this.userId}`);
            const messages = await response.json();
            
            if (messages && messages.length > 0) {
                const messagesContainer = document.getElementById('chat-messages');
                const currentMessages = messagesContainer.querySelectorAll('.bot-message, .user-message').length;
                
                // Если есть новые сообщения
                if (messages.length > currentMessages) {
                    const newMessages = messages.slice(currentMessages);
                    
                    newMessages.forEach(msg => {
                        if (msg.respondedBy && msg.respondedBy !== 'Бот') {
                            // Ответ от администратора
                            this.addAdminMessage(msg.message, msg.respondedBy);
                            
                            if (!this.isOpen) {
                                this.showNotificationBadge(1);
                            }
                        }
                    });
                }
            }
        } catch (error) {
            console.log('Ошибка проверки новых сообщений');
        }
    }

    addAdminMessage(message, adminName) {
        const messagesContainer = document.getElementById('chat-messages');
        const messageHTML = `
            <div class="bot-message admin-message">
                <div class="message-avatar">
                    <i class="fas fa-user-tie"></i>
                </div>
                <div class="message-content">
                    <div class="admin-name">${adminName}</div>
                    <div class="message-text">${this.formatBotMessage(message)}</div>
                    <div class="message-time">${new Date().toLocaleTimeString('ru-RU', {hour: '2-digit', minute: '2-digit'})}</div>
                </div>
            </div>
        `;
        
        messagesContainer.insertAdjacentHTML('beforeend', messageHTML);
        this.scrollToBottom();
    }
}

// Инициализация чата при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    window.supportChat = new SupportChat();
});