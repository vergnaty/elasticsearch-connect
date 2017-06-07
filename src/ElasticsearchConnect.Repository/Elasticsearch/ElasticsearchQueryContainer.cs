using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Repository.Elasticsearch
{
    public class ElasticsearchQueryContainer<TEntity>
        where TEntity : class
    {
        public QueryContainer Query { get; set; }
        public AggregationContainerDescriptor<TEntity> Aggregation { get; set; }
    }
}
