using System.Collections.Generic;

namespace MyBlog.Models.ViewModels
{
    public class ViewHome
    {
        public Post Feature { get; set; }
        public List<Post> Posts { get; set; }
        public List<Document> Documents { get; set; }
        public List<Category> Categories { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int MaxPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
