using ShoeShop.Paginations.Models;

namespace ShoeShop.Paginations.ViewModels {
    public class PaginationViewModel {
        public PaginationViewModel(Pagination pagination, RouteInfo routeInfo) {
            Pagination = pagination;
            Route = routeInfo;
        }

        public Pagination Pagination { get; private set; }
        public RouteInfo Route { get; private set; }
    }
}
