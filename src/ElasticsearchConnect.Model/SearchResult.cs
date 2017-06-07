using System.Collections.Generic;

namespace ElasticsearchConnect.Model
{
    public class SearchResult<TEntity> 
        where TEntity : class
    {
        /// <summary>
        /// The List of document retrived from the search Engine
        /// </summary>
        public IEnumerable<TEntity> Documents { get; set; }

        /// <summary>
        /// The List of aggregation retrived from the search Engine
        /// </summary>
        public IList<SearchAggregationItem> Aggregations { get; set; }
        
        /// <summary>
        /// The pagination informations
        /// </summary>
        public PagingParameter Paging { get; set; }
    }
}
