using MyBlog.Models;
using MyBlog.Utilities;
using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyBlog.Service
{
    public interface IElasticsearch
    {
        Task<SearchResult<T>> Search<T>(string index, string searchKey, int page, int pageSize) where T: class;
        Task<IEnumerable<string>> AutoComplete<T>(string key, string searchKey, int size) where T : class;
        Task<IndexResponse> Index<T>(T data, string indexName, string id) where T : class;
        Task<BaseResponse<T>> UpdateDocument<T>(T Document, string indexName) where T : class;
        Task<CreateIndexResponse> CreateIndexAsync<T>(string indexName) where T : class;
        Task<bool> CheckExistIndexAsync(string indexName);
        bool CheckExistIndex(string indexName);
        Task<CreateIndexResponse> CreatePostIndexAsync();
        Task<CreateIndexResponse> CreateDocumentIndexAsync();
    }
}
