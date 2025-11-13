using System.Collections.Generic;

namespace ShoeShop.Paginations.Models {
    public class RouteInfo {
        public RouteInfo(string page, Dictionary<string, string> routeData) {
            Page = page;
            Data = routeData;
        }
        public string Page { get; private set; }
        public Dictionary<string, string> Data { get; private set; }
    }
}
