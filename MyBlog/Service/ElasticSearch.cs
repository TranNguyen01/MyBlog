using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyBlog.Models;
using MyBlog.Utilities;
using Nest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Service
{
    public class ElasticSearch : IElasticsearch
    {
        private readonly IElasticClient _ESClient;
        public ElasticSearch()
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri("http://localhost:9200/")).DefaultIndex("Blog").RequestTimeout(TimeSpan.FromMinutes(2));
            _ESClient = new ElasticClient(settings);
            var response = _ESClient.Ping();
        }
        public ElasticSearch(IElasticClient esClient)
        {
            _ESClient = esClient;
            var response = _ESClient.Ping();
        }
        public async Task<IEnumerable<string>> AutoComplete<T>(string key, string searchKey, int size) where T : class
        {
            var esResults = await _ESClient.SearchAsync<Post>(s => s
                .Index("post")
                .Suggest(s => s
                    .Completion("suggestions", c => c
                       .Field(f=>f.Title)
                       .Prefix(searchKey)
                       .Fuzzy(fz=>fz.Fuzziness(Fuzziness.Auto))
                       .Size(10)
                    )
                )
            );
            //var result = from esr in esResults.Suggest[key]
            //             from option in esr.Options
            //             select option.Source.Name;
            //return result.ToList();
            return new List<string>();
        }


        public async Task<SearchResult<T>> Search<T>(string index, string searchKey, int page = 1, int pageSize = 10) where T : class
        {
            var esResult = await _ESClient.SearchAsync<T>(s => s
                .Index(index)
                .MinScore(0.3)
                .Query(q => q
                    .MultiMatch(ma => ma
                        .Query(searchKey)
                        .Analyzer("vi_analyzer")
                    )
                )
                .TrackScores(true)
                .Sort(sort=>sort.Field(f=>f.Field("_score").Descending()))
                .Skip((page - 1) * pageSize)
                .Take(pageSize));

            //var esResult = await _ESClient.SearchAsync<T>(s => s
            //    .Index(index)
            //    .Query(q => q
            //        .MultiMatch(ma => ma
            //            .Query(searchKey)
            //            .Analyzer("vi_analyzer")
            //        )
            //    )
            //    .TrackScores(true)
            //    .Sort(sort => sort.Field(f => f.Field("_score").Descending()))
            //    .Skip((page - 1) * pageSize)
            //    .Take(pageSize));

            return new SearchResult<T>
            {
                Code = 0,
                Total = (int)esResult.Total,
                Page = page,
                Data = esResult.Documents,
                ElapsedMilliseconds = (int)esResult.Took
            };
        }

        public async Task<IndexResponse> Index<T>(T data, string indexName, string id) where T : class
        {
            if (!_ESClient.Indices.Exists(indexName).Exists) return null;
            return await _ESClient.IndexAsync<T>(data, i => i.Index(indexName).Id(id));
        }

        public async Task<BaseResponse<T>> UpdateDocument<T>(T document, string indexName) where T : class
        {
            //UpdateResponse<T> esResponse = await _ESClient.UpdateAsync<T>(document.Id, doc => doc
            //     .Index(indexName)
            //     .Doc(document)
            // );
            //if(esResponse.Result == Result.Updated)
            // {
            //     return new BaseResponse<T>
            //     {
            //         Code = 0,
            //         message = "",
            //         Data = null
            //     };
            // }

            return new BaseResponse<T>
            {
                Code = 1,
                Data = null
            };
        }

        public async Task<CreateIndexResponse> CreatePostIndexAsync()
        {
            return await _ESClient.Indices.CreateAsync("post", index => index
                .Settings(se => se
                    .Analysis(a => a
                        .Analyzers(analyzer => analyzer
                            .Custom("vi_analyzer", analyzerDescriptor => analyzerDescriptor
                                .Tokenizer("vi_tokenizer")
                                .CharFilters("html_strip")
                                .Filters("lowercase", "icu_folding")))
                    )
                )
                .Map<Post>(mm => mm
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Title)
                            .Analyzer("vi_analyzer")
                            .SearchAnalyzer("vi_analyzer")
                        )
                        .Text(t => t
                            .Name(n => n.Content)
                            .Analyzer("vi_analyzer")
                            .SearchAnalyzer("vi_analyzer")
                        )
                    )
                )
            );
        }

        public async Task<CreateIndexResponse> CreateDocumentIndexAsync()
        {
            return await _ESClient.Indices.CreateAsync("document", index => index
                .Settings(se => se
                    .Analysis(a => a
                        .Analyzers(analyzer => analyzer
                            .Custom("vi_analyzer", analyzerDescriptor => analyzerDescriptor
                                .Tokenizer("vi_tokenizer")
                                .CharFilters("html_strip")
                                .Filters("lowercase", "icu_folding")))
                    )
                )
                .Map<Models.Document>(mm => mm
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Name)
                            .Analyzer("vi_analyzer")
                            .SearchAnalyzer("vi_analyzer")
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("vi_analyzer")
                            .SearchAnalyzer("vi_analyzer")
                        )
                    )
                )
            );
        }

        public async Task<bool> CheckExistIndexAsync(string indexName)
        {
            var esResponse = await _ESClient.Indices.ExistsAsync(indexName);
            return esResponse.Exists;
        }

        public bool CheckExistIndex(string indexName)
        {
            var esResponse = _ESClient.Indices.Exists(indexName);
            return esResponse.Exists;
        }

        public async Task<CreateIndexResponse> CreateIndexAsync<T>(string indexName) where T : class
        {
            return await _ESClient.Indices.CreateAsync(indexName, index => index
                .Settings(se => se
                    .Analysis(a => a
                        .Analyzers(analyzer => analyzer
                            .Custom("vi_analyzer", analyzerDescriptor => analyzerDescriptor
                                .Tokenizer("vi_tokenizer")
                                .CharFilters("html_strip")
                                .Filters("icu_folding")))
                    )
                )
            );
        }
    }
}
