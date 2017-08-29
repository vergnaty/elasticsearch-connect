using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticConnector.Model
{
    public class PagingParameter
    {
        /// <summary>
        /// The pagination index
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// the total items per page
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// the sort field
        /// </summary>
        public string SortField { get; set; }
        /// <summary>
        /// Sort by either ASC OR DESC
        /// </summary>
        public OrderBy Sortby { get; set; }
        /// <summary>
        /// the total documents 
        /// </summary>
        public long TotalCount { get; set; }
        /// <summary>
        /// time execution in milliseconds 
        /// </summary>
        public long TimeExecution { get; set; }
    }

    public enum OrderBy : short
    {
        Ascending = 0,
        Descending = 1
    }

}
