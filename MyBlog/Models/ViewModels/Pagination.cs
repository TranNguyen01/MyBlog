namespace MyBlog.Models.ViewModels
{
    public class Pagination
    {
        public int maxPage { get; set; }
        public int currentPage { get; set; } = 1;

        public string action { get; set; }

        public string controller { get; set; }
    }
}
