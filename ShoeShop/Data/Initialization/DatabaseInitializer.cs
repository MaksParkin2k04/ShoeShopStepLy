using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Models;

namespace ShoeShop.Data.Initialization {
    public class DatabaseInitializer {
        private const string Admin = "Admin";
        private const string Customer = "Customer";

        private static readonly Random random = new Random();

        public static async Task Initialize(ApplicationContext context, IUserStore<ApplicationUser> userStore, UserManager<ApplicationUser> userManager, IRoleStore<ApplicationRole> roleStore, RoleManager<ApplicationRole> roleManager) {

            await context.Database.EnsureDeletedAsync();

            if (await context.Database.EnsureCreatedAsync()) {
                await CreateUsers(userStore, userManager, roleStore, roleManager);
                await CreateProducts(context);

                IList<ApplicationUser> users = await userManager.GetUsersInRoleAsync(Customer);
                Product[] products = await context.Products.Include(p => p.Images).ToArrayAsync();
                await CreateOrders(context, users.ToArray(), products);
            }
        }

        private static async Task CreateProducts(ApplicationContext context) {
            string json = File.ReadAllText("Data/Initialization/PRODUCT_DATA.JSON");
            ProductDto[]? products = JsonSerializer.Deserialize<ProductDto[]>(json);

            List<Product> productList = new List<Product>();

            foreach (ProductDto productDto in products ?? Enumerable.Empty<ProductDto>()) {
                Product product = Product.Create(productDto.Name, productDto.IsSale, productDto.Price, productDto.Sizes, productDto.DateAdded, productDto.Description, productDto.Content);

                //for (int i = 0; i < productDto.Images?.Length; i++) {
                //    ImageDto imageDto = productDto.Images[i];
                //    product.AddImage(imageDto.Path, $"{i.ToString("D2")} - {imageDto.Alt}");
                //}

                foreach (ImageDto imageDto in productDto.Images ?? Enumerable.Empty<ImageDto>()) {
                    product.AddImage(imageDto.Path, imageDto.Alt);
                }

                context.Products.Add(product);
            }

            int count = await context.SaveChangesAsync();
        }

        private static async Task CreateUsers(IUserStore<ApplicationUser> userStore, UserManager<ApplicationUser> userManager, IRoleStore<ApplicationRole> roleStore, RoleManager<ApplicationRole> roleManager) {
            // Создаем роль администратора
            ApplicationRole adminRole = Activator.CreateInstance<ApplicationRole>();
            await roleStore.SetRoleNameAsync(adminRole, Admin, CancellationToken.None);
            IdentityResult createRoleResult = await roleManager.CreateAsync(adminRole);

            // Создаем роль клиента
            ApplicationRole customerRole = Activator.CreateInstance<ApplicationRole>();
            await roleStore.SetRoleNameAsync(customerRole, Customer, CancellationToken.None);
            IdentityResult customerRoleResult = await roleManager.CreateAsync(customerRole);

            await CreateUsers("admin", Admin, userStore, userManager, roleStore, roleManager);
            await CreateUsers("first", Customer, userStore, userManager, roleStore, roleManager);
            await CreateUsers("second", Customer, userStore, userManager, roleStore, roleManager);
            await CreateUsers("next", Customer, userStore, userManager, roleStore, roleManager);
        }

        private static async Task CreateUsers(string name, string role, IUserStore<ApplicationUser> userStore, UserManager<ApplicationUser> userManager, IRoleStore<ApplicationRole> roleStore, RoleManager<ApplicationRole> roleManager) {
            IUserEmailStore<ApplicationUser> emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            ApplicationUser user = Activator.CreateInstance<ApplicationUser>();

            string email = $"{name}@shoeshop.ru";
            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);

            // Создаем пользователя
            IdentityResult result = await userManager.CreateAsync(user, "qwerty");
            string? code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            IdentityResult confirmResult = await userManager.ConfirmEmailAsync(user, code);

            // Добавляем роль
            IdentityResult identityResult = await userManager.AddToRoleAsync(user, role);
        }

        private static async Task CreateOrders(ApplicationContext context, ApplicationUser[] users, Product[] products) {
            int coment = 1;
            foreach (ApplicationUser user in users) {
                int orderCount = random.Next(100, 300);
                for (int i = 0; i < orderCount; i++) {
                    int productCount = random.Next(1, 30);
                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    for (int x = 0; x < productCount; x++) {
                        int productIndex = random.Next(0, products.Length);
                        Product product = products[productIndex];
                        orderDetails.Add(OrderDetail.Create(product.Id, product.Images[0].Path, product.Name, product.Price));
                    }

                    OrderStatus[] orderStatuses = Enum.GetValues<OrderStatus>();
                    int statusIndex = random.Next(0, 4);
                    Order order = Order.Create(user.Id, RandomDateTime(), $"Коментарий к заказу {coment++}", RandomRecipient(), orderDetails);
                    order.SetStatus(orderStatuses[statusIndex]);

                    context.Orders.Add(order);
                }
            }

            await context.SaveChangesAsync();
        }

        private static DateTime RandomDateTime() {
            int year = 2024;
            int month = random.Next(1, 10);
            int day = random.Next(1, 28);
            int hour = random.Next(7, 21);
            int minute = random.Next(0, 60);
            int second = random.Next(0, 60);

            return new DateTime(year, month, day, hour, minute, second);
        }

        private static OrderRecipient RandomRecipient() {
            string[] names = new string[] {
                "Степан Степанович", "Михаил Петрович", "Вероника Михаиловна", "Надежда Сергеевна", "Игорь", "Татьяна", "Леонид", "Виктор", "Светлана", "Роман"
            };

            string[] sites = new string[] {
                "Цветочный бульвар", "Чернореченская", "2-й проезд Кривцова", "Авдеевский переулок", "Барнаульский переулок", "Бугурусланская", "Губерлинский проезд", "Инструментальная", "Интернациональная", "Хозяйственный переулок"
            };

            string phone = $"+7 (9{random.Next(1, 99).ToString("D2")}) {random.Next(100, 999).ToString("D3")}-{random.Next(0, 99).ToString("D2")}-{random.Next(0, 99).ToString("D2")}";

            return OrderRecipient.Create(names[random.Next(0, names.Length)], "Оренбург", sites[random.Next(0, sites.Length)], random.Next(1, 200).ToString(), random.Next(1, 200).ToString(), phone); ;
        }

        private static void UpdateJsonData() {
            string json = File.ReadAllText("Data/Initialization/PRODUCT_DATA.JSON");
            ProductDto[]? products = JsonSerializer.Deserialize<ProductDto[]>(json);

            if (products != null) {

                string[] names = Enum.GetNames(typeof(ProductSize));

                foreach (ProductDto product in products) {
                    do {
                        product.Sizes = ProductSize.Not;
                        int count = random.Next(0, names.Length);
                        for (int i = 0; i < count; i++) {
                            int index = random.Next(0, names.Length);
                            product.Sizes |= Enum.Parse<ProductSize>(names[index]);
                        }
                    } while (product.Sizes == ProductSize.Not);
                }

                ProductDto[] productDtos = products.Where(p => p.IsSale == false).ToArray();

                if (productDtos.Length > 10) {
                    HashSet<int> ids = new HashSet<int>();

                    do {
                        int index = random.Next(0, productDtos.Length);
                        ids.Add(index);
                    }
                    while (ids.Count != productDtos.Length - 3);

                    foreach (int index in ids) {
                        productDtos[index].Sizes = ProductSize.Not;
                    }
                }

                ProductDto[] productDtos2 = products.Where(p => p.Sizes == ProductSize.Not && p.IsSale).ToArray();

                JsonSerializerOptions options = new JsonSerializerOptions {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };

                json = JsonSerializer.Serialize(products, options);

            }
        }
    }
}
