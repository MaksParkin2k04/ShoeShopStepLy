using ShoeShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Data.Initialization {
    public static class TestDataInitializer {
        public static async Task InitializeAsync(ApplicationContext context) {
            if (await context.Products.AnyAsync()) {
                return; // Данные уже есть
            }
            
            // Получаем существующие категории
            var categories = await context.Categories.ToListAsync();
            if (!categories.Any()) {
                return; // Нет категорий - не добавляем товары
            }
            
            var menCategory = categories.FirstOrDefault(c => c.Name.Contains("Мужская") || c.Name.Contains("мужская"));
            var womenCategory = categories.FirstOrDefault(c => c.Name.Contains("Женская") || c.Name.Contains("женская"));
            var kidsCategory = categories.FirstOrDefault(c => c.Name.Contains("Детская") || c.Name.Contains("детская"));
            
            var products = new List<Product>();
            
            // Мужские кроссовки
            var menShoes = new[] {
                ("Nike Air Max 270", 12990, "Стильные мужские кроссовки Nike Air Max 270", "Комфортные кроссовки для повседневной носки с технологией Air Max"),
                ("Adidas Ultraboost 22", 15990, "Беговые кроссовки Adidas Ultraboost", "Профессиональные беговые кроссовки с технологией Boost"),
                ("Puma RS-X", 8990, "Ретро кроссовки Puma RS-X", "Стильные ретро кроссовки с яркими акцентами"),
                ("New Balance 574", 7990, "Классические кроссовки New Balance", "Легендарные кроссовки в классическом стиле"),
                ("Reebok Classic Leather", 6990, "Кожаные кроссовки Reebok Classic", "Классические белые кроссовки из натуральной кожи"),
                ("Converse Chuck Taylor", 5990, "Высокие кеды Converse", "Культовые высокие кеды в классическом стиле"),
                ("Vans Old Skool", 6490, "Скейтерские кеды Vans", "Популярные кеды для скейтбординга и повседневной носки"),
                ("ASICS Gel-Kayano", 13990, "Беговые кроссовки ASICS", "Профессиональные кроссовки для бега с гелевой амортизацией"),
                ("Under Armour HOVR", 9990, "Тренировочные кроссовки Under Armour", "Современные кроссовки для тренировок"),
                ("Jordan Air Jordan 1", 18990, "Баскетбольные кроссовки Jordan", "Легендарные баскетбольные кроссовки Michael Jordan")
            };
            
            if (menCategory != null) {
                foreach (var (name, price, desc, content) in menShoes) {
                    var product = Product.Create(name, true, price, ProductSize.All, DateTime.Now, desc, content, menCategory.Id);
                    products.Add(product);
                }
            }
            
            // Женские кроссовки
            var womenShoes = new[] {
                ("Nike Air Force 1 '07", 11990, "Женские кроссовки Nike Air Force 1", "Классические белые кроссовки для женщин"),
                ("Adidas Stan Smith", 8990, "Женские кеды Adidas Stan Smith", "Минималистичные белые кеды с зелеными акцентами"),
                ("Puma Cali", 7990, "Женские кроссовки Puma Cali", "Стильные женские кроссовки в калифорнийском стиле"),
                ("New Balance 327", 9490, "Ретро кроссовки New Balance 327", "Винтажные кроссовки с современными технологиями"),
                ("Reebok Club C 85", 6990, "Женские кеды Reebok Club C", "Элегантные белые кеды для повседневной носки"),
                ("Converse Chuck 70", 7490, "Премиум кеды Converse Chuck 70", "Улучшенная версия классических кед Converse"),
                ("Vans Authentic", 5490, "Классические кеды Vans", "Простые и стильные кеды для каждого дня"),
                ("ASICS Gel-Lyte III", 10990, "Женские кроссовки ASICS Gel-Lyte", "Комфортные кроссовки с раздельным языком"),
                ("Fila Disruptor II", 6490, "Массивные кроссовки Fila", "Трендовые массивные кроссовки в стиле 90-х"),
                ("Nike Blazer Mid '77", 9990, "Высокие кеды Nike Blazer", "Винтажные баскетбольные кеды Nike")
            };
            
            if (womenCategory != null) {
                foreach (var (name, price, desc, content) in womenShoes) {
                    var product = Product.Create(name, true, price, ProductSize.All, DateTime.Now, desc, content, womenCategory.Id);
                    products.Add(product);
                }
            }
            
            // Детские кроссовки
            var kidsShoes = new[] {
                ("Nike Air Max 90 Kids", 6990, "Детские кроссовки Nike Air Max", "Яркие детские кроссовки с технологией Air Max"),
                ("Adidas Superstar Kids", 5990, "Детские кеды Adidas Superstar", "Классические детские кеды с тремя полосками"),
                ("Puma Smash v2 Kids", 3990, "Детские кеды Puma Smash", "Простые и удобные кеды для детей"),
                ("New Balance 373 Kids", 4990, "Детские кроссовки New Balance", "Комфортные детские кроссовки для активных игр"),
                ("Reebok Royal Kids", 3490, "Детские кроссовки Reebok Royal", "Стильные детские кроссовки в классическом стиле"),
                ("Converse All Star Kids", 4490, "Детские кеды Converse", "Маленькие кеды в стиле взрослых моделей"),
                ("Vans Old Skool Kids", 4990, "Детские кеды Vans", "Популярные скейтерские кеды для детей"),
                ("ASICS Contend Kids", 5490, "Детские беговые кроссовки ASICS", "Легкие кроссовки для детского спорта"),
                ("Skechers Lights Kids", 4990, "Светящиеся кроссовки Skechers", "Яркие кроссовки с LED подсветкой"),
                ("Geox Respira Kids", 6490, "Дышащие кроссовки Geox", "Детские кроссовки с дышащей подошвой")
            };
            
            if (kidsCategory != null) {
                foreach (var (name, price, desc, content) in kidsShoes) {
                    var product = Product.Create(name, true, price, ProductSize.From26To32, DateTime.Now, desc, content, kidsCategory.Id);
                    products.Add(product);
                }
            }
            
            if (products.Any()) {
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}