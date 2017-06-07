using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Model
{
    public class AggregationItem
    {
        #region properties
        public string Name { get; set; }
        public int Size { get; set; } = 5;
        public AggregationRangeItem<DateTime>[] RangesDate { get; set; }
        public AggregationRangeItem<double>[] Ranges { get; set; }
        public List<string> ExcludedItems { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AggregationExpressionType Type { get; set; }
        public string SortField { get; set; } = "_count";
        public string SortType { get; set; } = "desc";
        public List<AggregationItem> Children { get; set; }

        #endregion

        #region constructor(s)
        public AggregationItem()
        {
        }
        public AggregationItem(string name)
        {
            this.Name = name;
        }
        #endregion
    }
}
