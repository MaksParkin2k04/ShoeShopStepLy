using ClosedXML.Excel;
using ShoeShop.Models;
using ShoeShop.Data;
using Microsoft.EntityFrameworkCore;

namespace ShoeShop.Services {
    public class ExportService {
        private readonly ApplicationContext _context;

        public ExportService(ApplicationContext context) {
            _context = context;
        }

        public async Task<byte[]> ExportStatisticsToExcel() {
            using var workbook = new XLWorkbook();
            
            await CreateDashboardSheet(workbook);
            await CreateSalesAnalysisSheet(workbook);
            await CreateProductAnalysisSheet(workbook);
            await CreateOrderStatusSheet(workbook);
            await CreateDetailedDataSheet(workbook);
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private async Task CreateDashboardSheet(XLWorkbook workbook) {
            var sheet = workbook.Worksheets.Add("📊 Дашборд");
            
            // Заголовок
            sheet.Range("A1:F1").Merge().Value = "ОТЧЕТ ПО ПРОДАЖАМ STEPLY";
            sheet.Range("A1:F1").Style.Font.Bold = true;
            sheet.Range("A1:F1").Style.Font.FontSize = 16;
            sheet.Range("A1:F1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            sheet.Range("A1:F1").Style.Font.FontColor = XLColor.White;
            
            // Дата отчета
            sheet.Cell("A2").Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            sheet.Range("A2:F2").Merge();
            
            // Ключевые метрики
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            var totalProducts = await _context.Products.CountAsync();
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            
            // Блок KPI
            sheet.Cell("A4").Value = "КЛЮЧЕВЫЕ ПОКАЗАТЕЛИ";
            sheet.Range("A4:F4").Merge().Style.Font.Bold = true;
            sheet.Range("A4:F4").Style.Fill.BackgroundColor = XLColor.LightGray;
            
            var kpiData = new[] {
                new { Metric = "💰 Общая выручка", Value = $"{totalRevenue:C}", Color = XLColor.Green },
                new { Metric = "📦 Всего заказов", Value = totalOrders.ToString(), Color = XLColor.Blue },
                new { Metric = "🛍️ Товаров в каталоге", Value = totalProducts.ToString(), Color = XLColor.Orange },
                new { Metric = "💵 Средний чек", Value = $"{avgOrderValue:C}", Color = XLColor.Purple }
            };
            
            for (int i = 0; i < kpiData.Length; i++) {
                var row = 5 + i;
                sheet.Cell(row, 1).Value = kpiData[i].Metric;
                sheet.Cell(row, 2).Value = kpiData[i].Value;
                sheet.Range($"A{row}:B{row}").Style.Fill.BackgroundColor = kpiData[i].Color;
                sheet.Range($"A{row}:B{row}").Style.Font.FontColor = XLColor.White;
                sheet.Range($"A{row}:B{row}").Style.Font.Bold = true;
            }
            
            sheet.Columns().AdjustToContents();
        }

        private async Task CreateSalesAnalysisSheet(XLWorkbook workbook) {
            var sheet = workbook.Worksheets.Add("📈 Анализ продаж");
            
            // Заголовок
            sheet.Cell("A1").Value = "АНАЛИЗ ПРОДАЖ ПО ДНЯМ";
            sheet.Range("A1:D1").Merge().Style.Font.Bold = true;
            sheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.DarkGreen;
            sheet.Range("A1:D1").Style.Font.FontColor = XLColor.White;
            
            // Заголовки таблицы
            sheet.Cell("A3").Value = "Дата";
            sheet.Cell("B3").Value = "Заказов";
            sheet.Cell("C3").Value = "Выручка";
            sheet.Cell("D3").Value = "Средний чек";
            sheet.Range("A3:D3").Style.Font.Bold = true;
            sheet.Range("A3:D3").Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Данные за последние 30 дней
            var salesData = await _context.Orders
                .Where(o => o.OrderDate >= DateTime.Now.AddDays(-30))
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
            
            for (int i = 0; i < salesData.Count; i++) {
                var row = 4 + i;
                var data = salesData[i];
                var avgCheck = data.OrderCount > 0 ? data.Revenue / data.OrderCount : 0;
                
                sheet.Cell(row, 1).Value = data.Date.ToString("dd.MM.yyyy");
                sheet.Cell(row, 2).Value = data.OrderCount;
                sheet.Cell(row, 3).Value = data.Revenue;
                sheet.Cell(row, 4).Value = avgCheck;
                
                sheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00₽";
                sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00₽";
            }
            
            // Создание диаграммы
            if (salesData.Any()) {
                var dataRange = sheet.Range($"A3:C{3 + salesData.Count}");
                var chart = sheet.Charts.Add(XLChartType.Line, 6, 6, 20, 15);
                chart.SetChartData(dataRange);
                chart.Title = "Динамика продаж за 30 дней";
            }
            
            sheet.Columns().AdjustToContents();
        }

        private async Task CreateProductAnalysisSheet(XLWorkbook workbook) {
            var sheet = workbook.Worksheets.Add("🏆 Топ товары");
            
            // Заголовок
            sheet.Cell("A1").Value = "ТОП-20 САМЫХ ПРОДАВАЕМЫХ ТОВАРОВ";
            sheet.Range("A1:E1").Merge().Style.Font.Bold = true;
            sheet.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.DarkOrange;
            sheet.Range("A1:E1").Style.Font.FontColor = XLColor.White;
            
            // Заголовки
            var headers = new[] { "Рейтинг", "Товар", "Продано шт.", "Выручка", "Доля в продажах" };
            for (int i = 0; i < headers.Length; i++) {
                sheet.Cell(3, i + 1).Value = headers[i];
            }
            sheet.Range("A3:E3").Style.Font.Bold = true;
            sheet.Range("A3:E3").Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Данные
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.Product.Id, oi.Product.Name, oi.Product.Price })
                .Select(g => new {
                    ProductName = g.Key.Name,
                    Quantity = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * g.Key.Price)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(20)
                .ToListAsync();
            
            var totalRevenue = topProducts.Sum(p => p.Revenue);
            
            for (int i = 0; i < topProducts.Count; i++) {
                var row = 4 + i;
                var product = topProducts[i];
                var share = totalRevenue > 0 ? (product.Revenue / totalRevenue) * 100 : 0;
                
                sheet.Cell(row, 1).Value = i + 1;
                sheet.Cell(row, 2).Value = product.ProductName;
                sheet.Cell(row, 3).Value = product.Quantity;
                sheet.Cell(row, 4).Value = product.Revenue;
                sheet.Cell(row, 5).Value = $"{share:F1}%";
                
                // Медали для топ-3
                if (i < 3) {
                    var medals = new[] { "🥇", "🥈", "🥉" };
                    sheet.Cell(row, 1).Value = medals[i];
                    sheet.Range($"A{row}:E{row}").Style.Fill.BackgroundColor = 
                        i == 0 ? XLColor.Gold : i == 1 ? XLColor.Silver : XLColor.FromArgb(205, 127, 50);
                }
                
                sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00₽";
            }
            
            sheet.Columns().AdjustToContents();
        }

        private async Task CreateOrderStatusSheet(XLWorkbook workbook) {
            var sheet = workbook.Worksheets.Add("📋 Статусы заказов");
            
            // Заголовок
            sheet.Cell("A1").Value = "РАСПРЕДЕЛЕНИЕ ЗАКАЗОВ ПО СТАТУСАМ";
            sheet.Range("A1:C1").Merge().Style.Font.Bold = true;
            sheet.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.DarkRed;
            sheet.Range("A1:C1").Style.Font.FontColor = XLColor.White;
            
            // Заголовки
            sheet.Cell("A3").Value = "Статус";
            sheet.Cell("B3").Value = "Количество";
            sheet.Cell("C3").Value = "Процент";
            sheet.Range("A3:C3").Style.Font.Bold = true;
            sheet.Range("A3:C3").Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Данные по статусам
            var statusData = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();
            
            var totalOrders = statusData.Sum(s => s.Count);
            
            for (int i = 0; i < statusData.Count; i++) {
                var row = 4 + i;
                var status = statusData[i];
                var percentage = totalOrders > 0 ? (double)status.Count / totalOrders * 100 : 0;
                
                sheet.Cell(row, 1).Value = status.Status;
                sheet.Cell(row, 2).Value = status.Count;
                sheet.Cell(row, 3).Value = $"{percentage:F1}%";
            }
            
            sheet.Columns().AdjustToContents();
        }

        private async Task CreateDetailedDataSheet(XLWorkbook workbook) {
            var sheet = workbook.Worksheets.Add("📄 Детальные данные");
            
            // Заголовок
            sheet.Cell("A1").Value = "ДЕТАЛЬНАЯ ИНФОРМАЦИЯ ПО ЗАКАЗАМ";
            sheet.Range("A1:G1").Merge().Style.Font.Bold = true;
            sheet.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            sheet.Range("A1:G1").Style.Font.FontColor = XLColor.White;
            
            // Заголовки
            var headers = new[] { "ID заказа", "Дата", "Клиент", "Статус", "Товаров", "Сумма", "Email" };
            for (int i = 0; i < headers.Length; i++) {
                sheet.Cell(3, i + 1).Value = headers[i];
            }
            sheet.Range("A3:G3").Style.Font.Bold = true;
            sheet.Range("A3:G3").Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Последние 100 заказов
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(100)
                .ToListAsync();
            
            for (int i = 0; i < orders.Count; i++) {
                var row = 4 + i;
                var order = orders[i];
                
                sheet.Cell(row, 1).Value = order.Id;
                sheet.Cell(row, 2).Value = order.OrderDate.ToString("dd.MM.yyyy HH:mm");
                sheet.Cell(row, 3).Value = order.CustomerName;
                sheet.Cell(row, 4).Value = order.Status;
                sheet.Cell(row, 5).Value = order.OrderItems.Sum(oi => oi.Quantity);
                sheet.Cell(row, 6).Value = order.TotalAmount;
                sheet.Cell(row, 7).Value = order.CustomerEmail;
                
                sheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00₽";
                
                // Цветовое кодирование статусов
                var statusColor = order.Status switch {
                    "Completed" => XLColor.LightGreen,
                    "Pending" => XLColor.LightYellow,
                    "Cancelled" => XLColor.LightPink,
                    _ => XLColor.White
                };
                sheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = statusColor;
            }
            
            sheet.Columns().AdjustToContents();
        }

        public byte[] ExportStatisticsToExcel(SalesStatistics stats, List<ProductSalesStatistics> products, 
            decimal conversionRate, decimal avgOrderValue, List<TopProduct> topProducts) {
            
            using var workbook = new XLWorkbook();
            
            // Общая статистика
            var statsSheet = workbook.Worksheets.Add("Общая статистика");
            statsSheet.Cell("A1").Value = "Показатель";
            statsSheet.Cell("B1").Value = "Значение";
            statsSheet.Cell("A2").Value = "Продано пар";
            statsSheet.Cell("B2").Value = stats.TotalQuantitySold;
            statsSheet.Cell("A3").Value = "Выручка";
            statsSheet.Cell("B3").Value = stats.TotalRevenue;
            statsSheet.Cell("A4").Value = "Затраты";
            statsSheet.Cell("B4").Value = stats.TotalCosts;
            statsSheet.Cell("A5").Value = "Прибыль";
            statsSheet.Cell("B5").Value = stats.NetProfit;
            statsSheet.Cell("A6").Value = "Конверсия (%)";
            statsSheet.Cell("B6").Value = conversionRate;
            statsSheet.Cell("A7").Value = "Средний чек";
            statsSheet.Cell("B7").Value = avgOrderValue;
            statsSheet.Range("A1:B1").Style.Font.Bold = true;
            
            // Топ товары
            var topSheet = workbook.Worksheets.Add("Топ товары");
            topSheet.Cell("A1").Value = "Товар";
            topSheet.Cell("B1").Value = "Количество";
            topSheet.Cell("C1").Value = "Выручка";
            topSheet.Range("A1:C1").Style.Font.Bold = true;
            
            for (int i = 0; i < topProducts.Count; i++) {
                topSheet.Cell(i + 2, 1).Value = topProducts[i].Name;
                topSheet.Cell(i + 2, 2).Value = topProducts[i].Quantity;
                topSheet.Cell(i + 2, 3).Value = topProducts[i].Revenue;
            }
            
            // Детальная статистика
            var detailSheet = workbook.Worksheets.Add("Детальная статистика");
            detailSheet.Cell("A1").Value = "Товар";
            detailSheet.Cell("B1").Value = "Продано";
            detailSheet.Cell("C1").Value = "Выручка";
            detailSheet.Cell("D1").Value = "Затраты";
            detailSheet.Cell("E1").Value = "Прибыль";
            detailSheet.Range("A1:E1").Style.Font.Bold = true;
            
            for (int i = 0; i < products.Count; i++) {
                detailSheet.Cell(i + 2, 1).Value = products[i].ProductName;
                detailSheet.Cell(i + 2, 2).Value = products[i].QuantitySold;
                detailSheet.Cell(i + 2, 3).Value = products[i].Revenue;
                detailSheet.Cell(i + 2, 4).Value = products[i].Costs;
                detailSheet.Cell(i + 2, 5).Value = products[i].Profit;
            }
            
            // Автоширина колонок
            workbook.Worksheets.ToList().ForEach(ws => ws.Columns().AdjustToContents());
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}