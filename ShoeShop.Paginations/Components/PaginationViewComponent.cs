using System.Collections.Generic;
using ShoeShop.Paginations.Models;
using ShoeShop.Paginations.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ShoeShop.Paginations.Components {
    public class PaginationViewComponent : ViewComponent {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPage">Индекс текущей страницы</param>
        /// <param name="elementsPerPage">Число элементов которое может быть отображено на странице</param>
        /// <param name="totalElementsCount">Общее число элементов</param>
        /// <param name="page">Страница к которой будет отправлен запрос</param>
        /// <param name="routeData">Аргументы запроса</param>
        /// <returns>Пагинатор</returns>
        public IViewComponentResult Invoke(int currentPage, int elementsPerPage, int totalElementsCount, string page, Dictionary<string, string> routeData) {
            Pagination pagination = new Pagination(currentPage, elementsPerPage, totalElementsCount);
            RouteInfo routeInfo = new RouteInfo(page, routeData);
            PaginationViewModel viewModel = new PaginationViewModel(pagination, routeInfo);
            return View(viewModel);
        }
    }
}

