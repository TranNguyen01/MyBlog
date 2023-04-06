namespace MyBlog.Utilities
{
    public class BaseResponse<T>
    {
        public int Code { get; set; }
        public string message { get; set; }
        public T Data { get; set; }
    }
}
