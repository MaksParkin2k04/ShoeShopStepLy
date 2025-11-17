using ShoeShop.Models;

namespace ShoeShop.Data.Initialization
{
    public static class ProductDataInitializer
    {
        public static async Task InitializeAsync(ApplicationContext context)
        {
            // Проверяем, есть ли уже данные
            if (context.Products.Any())
            {
                return; // База уже заполнена
            }
            
            // Функция для получения пути к изображению (существующему или заглушке)
            string GetImagePath(string imageName)
            {
                var imagePath = Path.Combine("wwwroot", "images", "products", imageName);
                if (File.Exists(imagePath))
                {
                    return $"images/products/{imageName}";
                }
                // Используем заглушку если изображение не найдено
                return "images/slon-and-moska.jpg";
            }

            // Создаем категории
            var categories = new List<Category>
            {
                Category.Create("Мужская обувь"),
                Category.Create("Женская обувь"),
                Category.Create("Детская обувь"),
                Category.Create("Спортивная обувь")
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Создаем товары с фотографиями
            var products = new List<Product>();
            
            // Мужская обувь
            var product1 = Product.Create(
                "Классические туфли Oxford",
                true,
                8500,
                ProductSize.From40To45,
                DateTime.Now,
                "Элегантные мужские туфли из натуральной кожи для деловых встреч",
                "Классические туфли Oxford из высококачественной натуральной кожи. Идеально подходят для деловых встреч, официальных мероприятий и повседневной носки в офисе.",
                categories[0].Id
            );
            product1.AddImage(GetImagePath("men_oxford_1.jpg"), "Классические туфли Oxford");
            products.Add(product1);
            var product2 = Product.Create(
                "Кроссовки Nike Air Max",
                true,
                12000,
                ProductSize.From39To46,
                DateTime.Now,
                "Спортивные кроссовки с технологией Air Max для максимального комфорта",
                "Инновационные кроссовки Nike Air Max с революционной технологией амортизации. Обеспечивают максимальный комфорт при беге и повседневной носке.",
                categories[3].Id
            );
            product2.AddImage(GetImagePath("nike_airmax_1.jpg"), "Кроссовки Nike Air Max");
            products.Add(product2);
            var product3 = Product.Create(
                "Ботинки Timberland",
                true,
                15000,
                ProductSize.From40To45,
                DateTime.Now,
                "Прочные мужские ботинки из нубука, водонепроницаемые",
                "Легендарные ботинки Timberland из премиального нубука. Водонепроницаемая конструкция и прочная подошва обеспечивают надежность в любых условиях.",
                categories[0].Id
            );
            product3.AddImage(GetImagePath("timberland_boots_1.jpg"), "Ботинки Timberland");
            products.Add(product3);

            // Женская обувь
            var product4 = Product.Create(
                "Туфли на каблуке",
                true,
                6500,
                ProductSize.From35To40,
                DateTime.Now,
                "Элегантные женские туфли на среднем каблуке из натуральной кожи",
                "Изысканные женские туфли на устойчивом каблуке средней высоты. Изготовлены из мягкой натуральной кожи, идеально подходят для офиса и вечерних мероприятий.",
                categories[1].Id
            );
            product4.AddImage(GetImagePath("women_heels_1.jpg"), "Туфли на каблуке");
            products.Add(product4);
            var product5 = Product.Create(
                "Балетки кожаные",
                true,
                4500,
                ProductSize.From35To40,
                DateTime.Now,
                "Удобные женские балетки из мягкой кожи для повседневной носки",
                "Комфортные балетки из мягкой натуральной кожи. Плоская подошва и эргономичная форма обеспечивают комфорт в течение всего дня.",
                categories[1].Id
            );
            product5.AddImage(GetImagePath("women_flats_1.jpg"), "Балетки кожаные");
            products.Add(product5);
            var product6 = Product.Create(
                "Сапоги зимние",
                true,
                9500,
                ProductSize.From35To40,
                DateTime.Now,
                "Теплые женские сапоги с натуральным мехом",
                "Стильные зимние сапоги с натуральным мехом внутри. Водоотталкивающая поверхность и нескользящая подошва для безопасности в зимний период.",
                categories[1].Id
            );
            product6.AddImage(GetImagePath("women_boots_1.jpg"), "Сапоги зимние");
            products.Add(product6);

            // Детская обувь
            var product7 = Product.Create(
                "Детские кроссовки",
                true,
                3500,
                ProductSize.From28To35,
                DateTime.Now,
                "Яркие детские кроссовки с липучками, легкие и удобные",
                "Красочные детские кроссовки с удобными липучками. Легкая конструкция и дышащие материалы обеспечивают комфорт активным детям.",
                categories[2].Id
            );
            product7.AddImage(GetImagePath("kids_sneakers_1.jpg"), "Детские кроссовки");
            products.Add(product7);
            var product8 = Product.Create(
                "Детские сандалии",
                true,
                2800,
                ProductSize.From26To32,
                DateTime.Now,
                "Летние детские сандалии с закрытым носком",
                "Безопасные летние сандалии с защищенным носком. Регулируемые ремешки и нескользящая подошва для активных игр на улице.",
                categories[2].Id
            );
            product8.AddImage(GetImagePath("kids_sandals_1.jpg"), "Детские сандалии");
            products.Add(product8);

            // Спортивная обувь
            var product9 = Product.Create(
                "Кроссовки Adidas Ultraboost",
                true,
                14000,
                ProductSize.From38To45,
                DateTime.Now,
                "Беговые кроссовки с технологией Boost для максимального возврата энергии",
                "Профессиональные беговые кроссовки Adidas с инновационной технологией Boost. Обеспечивают максимальный возврат энергии при каждом шаге.",
                categories[3].Id
            );
            product9.AddImage(GetImagePath("adidas_ultraboost_1.jpg"), "Кроссовки Adidas Ultraboost");
            products.Add(product9);
            var product10 = Product.Create(
                "Футбольные бутсы",
                true,
                8500,
                ProductSize.From38To45,
                DateTime.Now,
                "Профессиональные футбольные бутсы с шипами для натурального газона",
                "Профессиональные футбольные бутсы с оптимизированной системой шипов. Обеспечивают отличное сцепление с натуральным газоном и точность ударов.",
                categories[3].Id
            );
            product10.AddImage(GetImagePath("football_boots_1.jpg"), "Футбольные бутсы");
            products.Add(product10);

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}