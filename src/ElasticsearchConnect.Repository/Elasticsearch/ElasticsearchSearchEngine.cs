using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ElasticsearchConnect.Model;
using Nest;
using Elasticsearch.Net;
using ElasticsearchConnect.Repository.Elasticsearch;

namespace ElasticsearchConnect.Repository
{
    public class ElasticsearchSearchEngine<TEntity> : ISearchEngine<TEntity>
        where TEntity : class
    {
        #region internal property

        private string currentIndex;
        private string currentType;
        private string aliasName;
        private ElasticClient client;

        #endregion  

        /// <summary>
        /// Create new instance of <see cref="ElasticsearchSearchEngine"/>
        /// </summary>
        /// <param name="urls">the elasticsearch nodes Url</param>
        public ElasticsearchSearchEngine(string[] urls)
        {
            if (urls == null)
                throw new ArgumentNullException("urls", "cannot be null");

            this.client = this.Connect(urls);
        }

        #region ISearchEngine Implementations
        public void CreateCompletationSuggestIndex(string indexName)
        {
            throw new NotImplementedException();
        }

        public void CreateIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException(nameof(indexName));

            this.currentIndex = indexName;
            this.client.CreateIndex(indexName, c => c.Settings(s => s.Analysis(a => a.Analyzers(aa =>
            {
                aa.Custom("case_insensitive_sort", ss => ss.Filters("lowercase").Tokenizer("keyword"));
                aa.Custom("autocomplate", ss => ss.Filters("standard", "lowercase").Tokenizer("standard"));
                return aa;
            }))).Mappings(m => m.Map<TEntity>(cc => cc.AutoMap(new CustomElasticPropertyVisitor()))));
        }

        public bool DeleteByQuery(params CustomFilter[] query)
        {
            throw new NotImplementedException();
        }

        public void DropIndex(string indexName)
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(IndexName));
            client.DeleteIndex(indexName);
        }

        public SearchResult<TEntity> FullTextSearch(SearchQueryItem queryItem)
        {
            if (queryItem == null) throw new ArgumentNullException(nameof(queryItem));

            int from = ((queryItem.Pagination.Page - 1) * queryItem.Pagination.PageSize);

            ElasticsearchQueryContainer<TEntity> query = ElasticsearchQueryBuilder<TEntity>.Create().Build(queryItem);
            SortDescriptor<TEntity> sort = new SortDescriptor<TEntity>();

            ISearchResponse<TEntity> response = this.client.Search<TEntity>(i => i.Index(this.aliasName ?? this.currentIndex)
                                                           .Type(currentType)
                                                           .Source(d=> queryItem.Select !=null? d.Includes(cc=> cc.Fields(queryItem.Select)) : d)
                                                           .Size(queryItem.Pagination.PageSize)
                                                           .Query(q => query.Query)
                                                           .Sort(s => sort)
                                                           .Aggregations(agg => query.Aggregation)
                                                           .Highlight(h => h.Fields(ff => ff.Field("*")
                                                                            .Type(HighlighterType.Plain))
                                                                            .PreTags("<span class='highlight'>")
                                                                            .PostTags("</span>")
                                                                            .RequireFieldMatch(false)));

            SearchResult<TEntity> result = this.GetElasticSearchResult(response);
            return result;
        }

        public List<KeyValuePair<string, string>> GetFilterFields()
        {
            var mapping = client.GetMapping<TEntity>(d => d.Index(this.currentIndex).Type(this.currentType));

            Func<Type, string, string> getName = (type, value) => type == typeof(TextProperty) ? value + ".keyword" : value;

            var properties = mapping.Mappings
                                .FirstOrDefault().Value
                                .SingleOrDefault().Value
                                .Properties;

            var result = GetFieldsName(properties);
            return result;
        }

        public void Save(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            client.Index(entity, d => d.Index(this.currentIndex).Type(this.currentType));
        }

        public void SaveMany(IEnumerable<TEntity> entites)
        {
            if (entites == null) throw new ArgumentNullException(nameof(entites));

            client.IndexMany(entites, index: this.currentIndex, type: typeof(TEntity).Name);
            client.Refresh(Indices.All);
        }

        public void SaveMany(Dictionary<object, TEntity> entites)
        {
            var request = new BulkRequest()
            {
                Refresh = Refresh.True,
                Operations = new List<IBulkOperation>()
            };

            int count = 0;
            foreach (var item in entites)
            {
                var operation = new Nest.BulkIndexOperation<TEntity>(item.Value);
                operation.Parent = item.Key.ToString();
                operation.Index = currentIndex;
                operation.Type = currentType;
                request.Operations.Add(operation);
                count++;
            }
            client.Bulk(request);
        }

        public ISearchEngine<TEntity> SetAlias(string aliasName)
        {
            if (string.IsNullOrEmpty(aliasName)) throw new ArgumentNullException(nameof(aliasName));
            this.aliasName = aliasName;
            return this;
        }

        public ISearchEngine<TEntity> SetIndexName(string indexName)
        {
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            currentIndex = indexName;
            return this;
        }

        public ISearchEngine<TEntity> SetType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) throw new ArgumentNullException(nameof(typeName));
            currentType = typeName;
            return this;
        }

        public void SwapAlias(string oldIndex, string newIndex)
        {
            if (!string.IsNullOrEmpty(oldIndex))
                client.Alias(alias =>
                {
                    alias.Add(c => c.Index(newIndex).Alias(this.aliasName));
                    if (client.IndexExists(oldIndex).Exists)
                    {
                        alias.Remove(c => c.Index(oldIndex).Alias(aliasName));
                    }
                    return alias;
                });
            else
                client.Alias(a => a.Add(c => c.Index(newIndex).Alias(aliasName)));
        }

        #endregion

        #region private methods

        /// <summary>
        /// Connect into elasticsearch by given Nodes Url
        /// </summary>
        /// <param name="urls">the nodes url</param>
        /// <returns>the ElasticClient</returns>
        private ElasticClient Connect(params string[] urls)
        {
            try
            {
                List<Uri> uris = urls.Select(url => new Uri(url)).ToList();
                SniffingConnectionPool pools = new SniffingConnectionPool(uris);

                ConnectionSettings settings = new ConnectionSettings(pools).DefaultFieldNameInferrer(prop => prop)
                                                                           .ThrowExceptions();
                ElasticClient client = new ElasticClient(settings);
                return client;
            }
            catch (Exception)
            {
                //LOG
                throw;
            }
        }

        private SortDescriptor<TEntity> GetSortDescriptor(string sortField, OrderBy sortOrder)
        {
            SortDescriptor<TEntity> sortDesciptor = new SortDescriptor<TEntity>();
            if (!string.IsNullOrEmpty(sortField))
            {
                sortDesciptor = sortOrder == OrderBy.Ascending ? sortDesciptor.Ascending(sortField)
                                                   : sortDesciptor.Descending(sortField);
            }
            return sortDesciptor;
        }

        private IList<SearchAggregationItem> GetAggregations(IReadOnlyDictionary<string, IAggregate> source)
        {
            List<SearchAggregationItem> aggregations = new List<SearchAggregationItem>();
            foreach (var item in source)
            {
                SearchAggregationItem currentAgg = new SearchAggregationItem(item.Key);

                var currentBucket = item.Value as Nest.BucketAggregate;
                if (currentBucket != null && currentBucket.Items.Count() > 0)
                {
                    foreach (var agg in currentBucket.Items)
                    {
                        var aggBucket = agg as Nest.KeyedBucket<object>;
                        var aggregationItem = GetBucketAggregation(aggBucket);

                        // If current bucket dosent have another aggregation 
                        // means all aggregated data are belonging to the current aggregation
                        // *Otherwise the result belong to the Parent aggregation
                        //ex : Distinct Count (Term)
                        if (aggBucket.Aggregations == null)
                        {
                            currentAgg.Items.Add(aggregationItem);
                        }
                        else
                        {
                            aggregationItem.Parentkey = currentAgg.Key;
                            aggregations.Add(aggregationItem);
                        }
                    }
                }
                else
                {
                    var valueAgg = item.Value as Nest.ValueAggregate;
                    currentAgg.Count = valueAgg.Value;
                    aggregations.Add(currentAgg);
                }
                if (currentAgg.Items.Count() > 0) aggregations.Add(currentAgg);
            }
            return aggregations;
        }

        private SearchAggregationItem GetBucketAggregation(Nest.KeyedBucket<object> aggregate)
        {
            var aggregationItem = new SearchAggregationItem(aggregate.Key.ToString())
            {
                Count = aggregate.DocCount
            };

            if (aggregate.Aggregations != null)
            {
                //Check aggregated data [BucketAggregate] rappresent nested nodes
                if (aggregate.Aggregations.Any(d => (d.Value as Nest.BucketAggregate) != null))
                {
                    aggregationItem.Items = GetAggregations(aggregate.Aggregations);
                }

                //Get all value aggregated data (Value Type (Sum, Avg,Count etc..) 
                aggregate.Aggregations.Where(d => (d.Value as Nest.ValueAggregate) != null)
                                      .ForEach(dd =>
                                        {
                                            aggregationItem.Items.Add(new SearchAggregationItem(dd.Key)
                                            {
                                                Count = (dd.Value as ValueAggregate).Value,
                                            });
                                        });
            }
            return aggregationItem;
        }

        private SearchResult<TEntity> GetElasticSearchResult(ISearchResponse<TEntity> response)
        {
            var result = new SearchResult<TEntity>()
            {
                Documents = response.Hits.Select(d => d.Source),
                Aggregations = this.GetAggregations(response.Aggregations),
                Paging = new PagingParameter()
                {
                    TotalCount = response.Total,
                    TimeExecution = response.Took
                }
            };
            return result;
        }

        private List<KeyValuePair<string, string>> GetFieldsName(IProperties properties, string parent = null)
        {
            List<KeyValuePair<string, string>> fields = new List<KeyValuePair<string, string>>();
            foreach (var item in properties)
            {
                fields.AddRange(this.GetProperties(item, parent));
            }
            return fields;
        }

        private List<KeyValuePair<string, string>> GetProperties(KeyValuePair<PropertyName, IProperty> property, string parent = null)
        {
            List<KeyValuePair<string, string>> properties = new List<KeyValuePair<string, string>>();
            string parentName = parent == null ? "" : $"{parent}.";
            if (property.Value is TextProperty)
            {
                string key = $"{parentName}{property.Key.Name}.keyword";
                properties.Add(new KeyValuePair<string, string>(key, $"{parentName}{property.Key.Name}"));
            }
            else if (property.Value is ObjectProperty)
            {
                var result = GetObjectProperties(property.Value, $"{parentName}{property.Key.Name}");
                properties.AddRange(result);
            }
            else if (property.Value is NestedProperty)
            {
                var result = GetNestedProperties(property.Value, $"{parentName}{property.Key.Name}");
                properties.AddRange(result);
            }
            else
            {
                string key = $"{parentName}{property.Key.Name}";
                properties.Add(new KeyValuePair<string, string>(key, key));
            }
            return properties;
        }

        private List<KeyValuePair<string, string>> GetObjectProperties(IProperty property, string key)
        {
            var parentObject = (property as ObjectProperty);
            var results = this.GetFieldsName(parentObject.Properties, key);
            return results;
        }

        private List<KeyValuePair<string, string>> GetNestedProperties(IProperty property, string key)
        {
            var parentObject = (property as NestedProperty);
            var results = this.GetFieldsName(parentObject.Properties, key);
            return results;
        }

        #endregion
    }
}
