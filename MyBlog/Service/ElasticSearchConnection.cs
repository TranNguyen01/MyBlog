using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MyBlog.Service
{
    public class ElasticSearchConnection
    {
        private ElasticClient _client;
        public ElasticSearchConnection(string url)
        {
            Init(url);
        }

        private void Init(string url)
        {
            var setting = new ConnectionSettings(new Uri(url)).DefaultIndex("defaultindex");
            _client = new ElasticClient(setting);
        }
        private string ValidatorSortBy<T>(string sortBy) where T : class, new()
        {
            string fieldSortKey = "id.keyword";
            if (sortBy != null)
            {
                var listOfFieldNames = typeof(T).GetProperties().Select(f => f.Name).ToList();
                foreach (string field in listOfFieldNames)
                {
                    if (sortBy.ToLower() == field.ToLower())
                    {
                        fieldSortKey = sortBy + ".keyword";
                        if (typeof(T).GetProperty(field).PropertyType.Name.ToLower() != "string") fieldSortKey = sortBy;
                        break;
                    }
                }
            }
            return fieldSortKey;
        }
        public ISearchResponse<T> QueryList<T>(string searchKey, string index, string sortBy, string sortOrder) where T : class, new()
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            string fieldSortKey = ValidatorSortBy<T>(sortBy);
            var fieldSortBy = new Field(fieldSortKey);
            var order = sortOrder == "desc" ? SortOrder.Descending : SortOrder.Ascending;
            return _client
               .Search<T>(s => s
                    .Index(index)
                    .Query(q => q.MultiMatch(c => c.Query(searchKey)))
                    .Sort(so => so.Field(fieldSortBy, order))
                    .TrackScores(true)
                );
        }

        public ISearchResponse<T> QueryListPaging<T>(string searchKey, string index, int pageIndex, int pageSize, string sortBy, string sortOrder) where T : class, new()
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            //var fieldSortBy = sortBy == null ? new Field("id.keyword") : new Field(sortBy + ".keyword");
            string fieldSortKey = ValidatorSortBy<T>(sortBy);
            var fieldSortBy = new Field(fieldSortKey);
            var order = sortOrder == "desc" ? SortOrder.Descending : SortOrder.Ascending;
            var maxScore = _client
               .Search<T>(s => s
                    .Index(index)
                    .Query(q => q.MultiMatch(c => c.Query(searchKey)))
                    .TrackScores(true)
                ).MaxScore;
            return _client
               .Search<T>(s => s
                    .Index(index)
                    .Query(q => q.MultiMatch(c => c.Query(searchKey)))
                    .TrackScores(true)
                    .MinScore(0.7 * maxScore)
                    .Sort(so => so.Field(fieldSortBy, order))
                    .From((pageIndex <= 1 ? 0 : pageIndex - 1) * pageSize)
                    .Size(pageSize)
                );
        }

        public GetResponse<T> GetById<T>(string id, string index) where T : class
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            return _client.Get<T>(id, g => g.Index(index));
        }

        public CreateIndexResponse CreateIndex<T>(string index) where T : class
        {
            return _client.Indices.Create(index, c => c
                .Map<T>(m => m
                    .AutoMap()
                )
            );
        }
        public async Task<BulkResponse> Bulk<T>(List<T> value, string index) where T : class
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            return await _client.BulkAsync(b => b.Index(index).IndexMany(value));
        }
        public IndexResponse Insert<T>(T value, string index, string id) where T : class
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            return _client.Index<T>(value, i => i.Index(index).Id(id));
        }
        public DeleteResponse Delete<T>(string id, string index) where T : class
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            return _client.Delete<T>(id, s => s.Index(index));
        }
        public DeleteIndexResponse DeleteIndex<T>(string index) where T : class
        {
            return _client.Indices.Delete(index);
        }
        public DeleteResponse DeleteDynamic<T>(string id, string index) where T : class
        {
            if (!_client.Indices.Exists(index).Exists) return null;
            return _client.Delete<dynamic>(id, s => s.Index(index));
        }
        public bool ExistIndex(string index)
        {
            return _client.Indices.Exists(index).Exists;
        }

    }
}
