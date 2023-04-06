using System.Collections;
using System.Collections.Generic;

namespace MyBlog.Models
{
    public class SearchResult<T>
    {
        public int Code { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public IEnumerable<T> Data { get; set; }
        public int ElapsedMilliseconds { get; set; }
    }
}
