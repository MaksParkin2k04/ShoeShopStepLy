using System;

namespace ShoeShop.Paginations.Models {
    /// <summary>
    /// Пагинация
    /// </summary>
    public class Pagination {
        /// <summary>
        /// Создает объект ExPagination.
        /// </summary>
        /// <param name="currentPage">Текущая страница</param>
        /// <param name="numberElementsPerPage">Количество элементов на странице</param>
        /// <param name="totalNumberElements">Общее количество элементов</param>
        public Pagination(int currentPage, int numberElementsPerPage, int totalNumberElements) {
            if (currentPage < 1) { throw new ArgumentOutOfRangeException(nameof(currentPage)); }
            if (numberElementsPerPage < 1) { throw new ArgumentOutOfRangeException(nameof(numberElementsPerPage)); }
            if (totalNumberElements < 0) { throw new ArgumentOutOfRangeException(nameof(totalNumberElements)); }

            CurrentPage = currentPage;
            ElementsPerPage = numberElementsPerPage;
            TotalElementsCount = totalNumberElements;

            PageCount = totalNumberElements / numberElementsPerPage;
            if ((totalNumberElements % numberElementsPerPage) > 0) {
                PageCount++;
            }
        }

        /// <summary>
        /// Количество злементов на странице.
        /// </summary>
        public int ElementsPerPage { get; private set; }

        /// <summary>
        /// Общее количество злементов.
        /// </summary>
        public int TotalElementsCount { get; private set; }

        /// <summary>
        /// Количество страниц.
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// Текущая страница.
        /// </summary>
        public int CurrentPage { get; private set; }

        /// <summary>
        /// Определяет наличие пагинации
        /// </summary>
        public bool HasPagination { get { return PageCount > 1; } }

        /// <summary>
        /// Показывает можно ли перейти на предыдущую страницу.
        /// </summary>
        public bool HasPrevious { get { return HasPagination && CurrentPage > 1; } }

        /// <summary>
        /// Показывает можно ли перейти на следующую страницу.
        /// </summary>
        public bool HasNext { get { return HasPagination && CurrentPage < PageCount; } }

        /// <summary>
        /// Предыдущая страница.
        /// </summary>
        public int Previous { get { return HasPrevious ? CurrentPage - 1 : -1; } }

        /// <summary>
        /// Следующая страница.
        /// </summary>
        public int Next { get { return HasNext ? CurrentPage + 1 : -1; } }

        /// <summary>
        /// Определяет наличие первого разделителя
        /// </summary>
        public bool HasFirstSeparator { get { return PageCount > 8 && CurrentPage > 4; } }

        /// <summary>
        /// Определяет наличие последнего разделителя
        /// </summary>
        public bool HasLastSeparator { get { return PageCount > 8 && CurrentPage + 3 < PageCount; } }

        /// <summary>
        /// Первый элемент
        /// </summary>
        public int First { get { return 1; } }

        /// <summary>
        /// Последний элемент
        /// </summary>
        public int Last { get { return PageCount; } }

        private int? start;
        /// <summary>
        /// Первый элемент блока
        /// </summary>
        public int Start {
            get {
                if (start == null) {
                    if (PageCount < 2) {
                        start = -1;
                    } else if (!HasLastSeparator && !HasFirstSeparator) {
                        start = 1;
                    } else if (HasLastSeparator && HasFirstSeparator) {
                        start = CurrentPage - 2;
                    } else if (HasLastSeparator && !HasFirstSeparator) {
                        start = 1;
                    } else if (!HasLastSeparator && HasFirstSeparator) {
                        start = PageCount - 5;
                    } else {
                        start = -1;
                    }
                }
                return start.Value;
            }
        }


        private int? end;
        /// <summary>
        /// Последний элемент блока
        /// </summary>
        public int End {
            get {
                if (end == null) {
                    if (PageCount < 2) {
                        end = -1;
                    } else if (!HasLastSeparator && !HasFirstSeparator) {
                        end = PageCount;
                    } else if (HasLastSeparator && HasFirstSeparator) {
                        end = CurrentPage + 2;
                    } else if (HasLastSeparator && !HasFirstSeparator) {
                        end = 1 + 5;
                    } else if (!HasLastSeparator && HasFirstSeparator) {
                        end = PageCount;
                    } else {
                        end = -1;
                    }
                }

                return end.Value;
            }
        }
    }
}
