using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface IResponseCacheService
    {
        Task SetResponseCacheAsync(string key, object value, TimeSpan timespan);

        Task<string> GetResponseCacheAsync(string key);

        Task ClearResponseCacheAsync(string key);

        string generateResponseCacheKey(string path, List<(string, string)> query);
    }
}
