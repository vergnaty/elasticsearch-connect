using System.Collections.Generic;

namespace ElasticsearchConnect.Model
{
    public class SearchQueryItem
    {
        /// <summary>
        /// Get or Set the fulltext 
        /// </summary>
        public string Fulltext { get; set; }

        /// <summary>
        /// Get or Set the list or properties to be returned as response object
        /// </summary>
        public string[] Select{ get; set; }
      
        /// <summary>
        /// Get or Set the aggregation items
        /// </summary>
        public List<AggregationItem> Aggregations { get; set; }
       
        /// <summary>
        /// Get or Set the Custom Filters
        /// </summary>
        public List<CustomFilter> Filters { get; set; }

        /// <summary>
        /// Get or set the Pagination
        /// </summary>
        public PagingParameter Pagination { get; set; }

        public SearchQueryItem()
        {
            this.Aggregations = new List<AggregationItem>();
            this.Filters = new List<CustomFilter>();
        }
    }
}
