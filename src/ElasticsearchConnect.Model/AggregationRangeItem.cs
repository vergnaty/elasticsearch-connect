using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ElasticsearchConnect.Model
{
    public class AggregationRangeItem<TItem>
    {
        public TItem From { get; set; }
        public TItem To { get; set; }

        public AggregationRangeItem()
        {
        }
        public AggregationRangeItem(TItem from, TItem to)
        {
            this.From = from;
            this.To = to;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AggregationExpressionType : short
    {
        /// <summary>
        /// Term aggregation (Count distinct)
        /// See <url>https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-bucket-terms-aggregation.html</url>
        /// </summary>
        Count,
        /// <summary>
        /// Range aggregation
        /// <url></url>
        /// </summary>
        Range,
        /// <summary>
        /// Sum aggregation
        /// See <url>https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-sum-aggregation.html</url>
        /// </summary>
        Sum,
        /// <summary>
        /// Average aggregation
        /// See <url>https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-avg-aggregation.html</url>
        /// </summary>
        Avg,
        /// <summary>
        /// Min aggregation
        /// See <url>https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-min-aggregation.html</url>
        /// </summary>
        Min,
        /// <summary>
        /// Max aggregation
        /// See <url>https://www.elastic.co/guide/en/elasticsearch/reference/current/search-aggregations-metrics-max-aggregation.html</url>
        /// </summary>
        Max
    }
}
