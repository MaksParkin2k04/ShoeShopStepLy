using ShoeShop.Models;

namespace ShoeShop.Data {
    public static class OrderQueryable {
        public static IQueryable<Order> OrderByDate(this IQueryable<Order> orders, OrderSorting sorting) {
            switch (sorting) {
                case OrderSorting.ByDateDesc:
                    return orders.OrderByDescending(o => o.CreatedDate);
                case OrderSorting.ByDateAsc:
                    return orders.OrderBy(p => p.CreatedDate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sorting));
            }
        }

        public static IQueryable<Order> Page(this IQueryable<Order> orders, int start, int take) {
            if (take == 0) { throw new ArgumentOutOfRangeException(nameof(take)); }

            if (start != 0) {
                orders = orders.Skip(start * take);
            }

            return orders.Take(take);
        }

        public static IQueryable<Order> StatusFilter(this IQueryable<Order> orders, OrderStatusFilter filter) {
            switch (filter) {
                case OrderStatusFilter.All:
                    return orders;
                case OrderStatusFilter.Active:
                    return orders.Where(o => o.Status != OrderStatus.Completed);
                case OrderStatusFilter.Created:
                    return orders.Where(o => o.Status == OrderStatus.Created);
                case OrderStatusFilter.Paid:
                    return orders.Where(o => o.Status == OrderStatus.Paid);
                case OrderStatusFilter.Processing:
                    return orders.Where(o => o.Status == OrderStatus.Processing);
                case OrderStatusFilter.AwaitingShipment:
                    return orders.Where(o => o.Status == OrderStatus.AwaitingShipment);
                case OrderStatusFilter.Shipped:
                    return orders.Where(o => o.Status == OrderStatus.Shipped);
                case OrderStatusFilter.InTransit:
                    return orders.Where(o => o.Status == OrderStatus.InTransit);
                case OrderStatusFilter.Arrived:
                    return orders.Where(o => o.Status == OrderStatus.Arrived);
                case OrderStatusFilter.ReadyForPickup:
                    return orders.Where(o => o.Status == OrderStatus.ReadyForPickup);
                case OrderStatusFilter.Completed:
                    return orders.Where(o => o.Status == OrderStatus.Completed);
                case OrderStatusFilter.Canceled:
                    return orders.Where(o => o.Status == OrderStatus.Canceled);
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }

        public static IQueryable<Order> CustomerFilter(this IQueryable<Order> orders, Guid customerId) {
            return orders.Where(o => o.CustomerId == customerId);
        }
    }
}
