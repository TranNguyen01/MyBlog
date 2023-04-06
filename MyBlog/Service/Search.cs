//using MyBlog.Utilities;
//using Nest;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System;

//namespace MyBlog.Service
//{
//    public class ElasticSearch
//    {
//        private readonly string _host;
//        private ElasticClient _client;
//        private ElasticSearchConnection _elasticSearchRep;
//        public ElasticSearch(String configuration)
//        {
//            _host = configuration;
//            _elasticSearchRep = new ElasticSearchConnection(_host);
//        }
//        public List<T> QueryList<T>(string content, string searchKey, string index, string sortBy, string sortOrder) where T : class, new()
//        {
//            var searchResponse = _elasticSearchRep.QueryList<T>(searchKey, index, sortBy, sortOrder);
//            if (searchResponse == null || searchResponse?.ApiCall.HttpStatusCode != 200) return null;
//            List<T> colResult = new List<T> { };
//            var dev = searchResponse.MaxScore;
//            foreach (var item in searchResponse.Hits)
//            {
//                if (item.Score < dev * 0.7) continue;
//                colResult.Add(item.Source);
//            }
//            return colResult;
//        }
//        public ISearchResponse<T> QueryListRaw<T>(string searchKey, string index, string sortBy, string sortOrder) where T : class, new()
//        {
//            var searchResponse = _elasticSearchRep.QueryList<T>(searchKey, index, sortBy, sortOrder);
//            return searchResponse;
//        }

//        public T GetById<T>(string content, string id, string index) where T : class
//        {
//            var getResponse = _elasticSearchRep.GetById<T>(id, index);
//            if (getResponse == null || getResponse?.ApiCall.HttpStatusCode != 200) return null;
//            return getResponse.Source;
//        }
//        public async Task<T> Bulk<T>(string content, List<T> value, string index) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index))
//            {
//                var createIndexResponse = _elasticSearchRep.CreateIndex<T>(index);
//                if (createIndexResponse == null
//                    || !(createIndexResponse?.ApiCall.HttpStatusCode == 200 || createIndexResponse?.ApiCall.HttpStatusCode == 201))
//                {
//                    return null;
//                }
//            }
//            var indexRespond = await _elasticSearchRep.Bulk<T>(value, index);
//            if (indexRespond == null
//                || !(indexRespond?.ApiCall.HttpStatusCode == 200 || indexRespond?.ApiCall.HttpStatusCode == 201))
//            {
//                return null;
//            }

//            return null;
//        }
//        public T Create<T>(string content, T value, string index, string id) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index))
//            {
//                var createIndexResponse = _elasticSearchRep.CreateIndex<T>(index);
//                if (createIndexResponse == null
//                    || !(createIndexResponse?.ApiCall.HttpStatusCode == 200 || createIndexResponse?.ApiCall.HttpStatusCode == 201))
//                {
//                    return null;
//                }
//            }
//            var indexRespond = _elasticSearchRep.Insert<T>(value, index, id);
//            if (indexRespond == null
//                || !(indexRespond?.ApiCall.HttpStatusCode == 200 || indexRespond?.ApiCall.HttpStatusCode == 201))
//            {
//                return null;
//            }
//            return null;
//        }
//        public IndexResponse CreateRaw<T>(string content, T value, string index, string id) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index))
//            {
//                var createIndexResponse = _elasticSearchRep.CreateIndex<T>(index);
//                if (createIndexResponse == null || createIndexResponse?.ApiCall.HttpStatusCode != 200) return null;
//            }

//            var indexRespond = _elasticSearchRep.Insert<T>(value, index, id);
//            if (indexRespond == null || indexRespond?.ApiCall.HttpStatusCode != 201) return null;

//            return indexRespond;
//        }
//        public T Update<T>(string content, T value, string index, string id) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index)) return null;
//            var indexRespond = _elasticSearchRep.Insert<T>(value, index, id);
//            if (indexRespond == null || !(indexRespond?.ApiCall.HttpStatusCode == 201 || indexRespond?.ApiCall.HttpStatusCode == 200)) return null;

//            return null;
//        }
//        public IndexResponse UpdateRaw<T>(string content, T value, string index, string id) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index)) return null;
//            var indexRespond = _elasticSearchRep.Insert<T>(value, index, id);
//            if (indexRespond == null || indexRespond?.ApiCall.HttpStatusCode != 201) return indexRespond;

//            return indexRespond;
//        }
//        public T Delete<T>(string content, string id, string index) where T : class
//        {
//            DeleteResponse resp = _elasticSearchRep.Delete<T>(id, index);
//            if (resp == null || resp?.ApiCall.HttpStatusCode != 200) return null;
//            return null;
//        }
//        public T DeleteRaw<T>(string content, string id, string index) where T : class
//        {
//            DeleteResponse resp = resp = _elasticSearchRep.DeleteDynamic<T>(id, index);
//            if (resp == null || resp?.ApiCall.HttpStatusCode != 200) return null;
//            return null;
//        }
//        public T ModifyData<T>(string index, string content, List<KeyValuePair<string, T>> data) where T : class
//        {
//            var _code = 1;
//            var subcontent = "";
//            if (!_elasticSearchRep.ExistIndex(index))
//            {
//                var createIndexResponse = _elasticSearchRep.CreateIndex<T>(index);
//                if (createIndexResponse == null
//                    || !(createIndexResponse?.ApiCall.HttpStatusCode == 200 || createIndexResponse?.ApiCall.HttpStatusCode == 201))
//                {
//                    _code = 0;
//                    subcontent = "Tạo index mới";
//                }
//            }
//            else
//            {
//                DeleteIndexResponse resp = resp = _elasticSearchRep.DeleteIndex<T>(index);
//                if (resp == null
//                    || !(resp?.ApiCall.HttpStatusCode == 200 || resp?.ApiCall.HttpStatusCode == 201))
//                {
//                    _code = 1;
//                    subcontent = "Xóa dữ liệu và index cũ";
//                }
//            }
//            foreach (var item in data)
//            {
//                if (_code != 0) break;
//                var createRep = Create<T>(content, item.Value, index, item.Key);
//                if (createRep == null)
//                {
//                    _code = 1;
//                    subcontent = "Tạo mới dữ liệu và index";
//                }
//            }

//            return null;
//        }

//        public T DeleteIndex<T>(string content, string index) where T : class
//        {
//            if (!_elasticSearchRep.ExistIndex(index))
//            {
//                return null;
//            }
//            DeleteIndexResponse resp = resp = _elasticSearchRep.DeleteIndex<T>(index);
//            if (resp == null || resp?.ApiCall.HttpStatusCode != 200) return null;
//            return null;
//        }
//    }
//}
