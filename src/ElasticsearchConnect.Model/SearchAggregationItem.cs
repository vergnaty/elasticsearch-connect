using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Model
{
    public class SearchAggregationItem
    {
        public string Key { get; private set; }
        public long? OtherDocuments { get; set; }
        public double? Count { get; set; }
        public string Parentkey { get; set; }
        public IList<SearchAggregationItem> Items { get; set; }

        public SearchAggregationItem(string key)
        {
            this.Key = key;
            this.Items = new List<SearchAggregationItem>();
        }
    }
}
