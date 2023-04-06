using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Utilities
{
    public class PaginatedList<T>
    {
        public int CurrentPage { get; set; }
        public int MaxItems { get; set; }
        public int MaxPage { get; set; }
        public int PageSize { get; set; }
        public List<T> Items { get; set; }

        public static async Task<PaginatedList<T>> CreatePaginatedList<T>(IQueryable<T> data, int currentPage, int pageSize){
            int maxItems = data.Count();
            int maxPage = maxItems % pageSize == 0 ? maxItems / pageSize : maxItems / pageSize + 1;
            List<T> items = await data.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>
            {
                CurrentPage = currentPage,
                MaxPage = maxPage,
                MaxItems = maxItems,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
