using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ElasticsearchConnect.Repository;
using ElasticsearchConnect.Model;
using ElasticsearchConnect.Api.Infrastructure;

namespace ElasticsearchConnect.Api.Controllers
{
    [Route("api/[controller]")]
    [CustomExceptionFilter]
    public class SearchController : Controller
    {
        ISearchEngine<object> searchEngine;
        public SearchController(ISearchEngine<object> engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            this.searchEngine = engine;
        }

        [Route("{indexName}/{type}")]
        [HttpPost]
        public SearchResult<object> Post(string indexName, string type, [FromBody]SearchQueryItem query)
        {
            return this.ExecuteSearch(indexName, type, query);
        }

        [Route("default-filter/{indexName}/{type}/{appname}")]
        public SearchResult<object> GetDefaultFilter(string indexName, string type, string appname)
        {
            SearchQueryItem query = new SearchQueryItem();
            query.Filters = new List<CustomFilter>();
            query.Pagination = new PagingParameter() { Page = 1, PageSize = 1 };
            query.Filters.Add(new CustomFilter() { Name = "_id", Value = appname });

            return this.ExecuteSearch(indexName, type, query);
        }

        [Route("fields/{dataname}/{type}")]
        public List<KeyValuePair<string, string>> GetFilterFields(string dataname, string type)
        {
            searchEngine.SetIndexName(dataname);
            searchEngine.SetType(type);
            return searchEngine.GetFilterFields();
        }

        private SearchResult<object> ExecuteSearch(string indexName, string type, SearchQueryItem query)
        {
            var result = searchEngine.SetIndexName(indexName)
                                     .SetType(type)
                                     .FullTextSearch(query);
            return result;
        }
    }
}
