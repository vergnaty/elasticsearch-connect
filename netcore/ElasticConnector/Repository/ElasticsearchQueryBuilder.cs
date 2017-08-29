using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;
using ElasticConnector.Model;

namespace ElasticConnector.Repository
{
    public class ElasticsearchQueryBuilder<TEntity>
                : ISearchQueryBuilder<ElasticsearchQueryContainer<TEntity>>
                , IBaseQueryBuild<QueryContainer> where TEntity : class
    {

        private static Dictionary<Operations, Func<string, string, QueryContainer>> queryBuilder;

        /// <summary>
        /// Create new instance of <see cref="ElasticsearchQueryBuilder"/>
        /// </summary>
        /// <returns>ISearchQueryBuilder of <see cref="ElasticsearchSearchEngine"/></returns>
        public static ISearchQueryBuilder<ElasticsearchQueryContainer<TEntity>> Create()
        {
            var instance = new ElasticsearchQueryBuilder<TEntity>();
            instance.SetDelegateExecutor();
            return instance;
        }

        #region ISearchQueryBuilder Implementation
        public ElasticsearchQueryContainer<TEntity> Build(SearchQueryItem query)
        {
            ElasticsearchQueryContainer<TEntity> container = new ElasticsearchQueryContainer<TEntity>();
            container.Query = this.GetQueryContainer(query);
            container.Aggregation = this.GetAggregation(query.Aggregations);
            return container;
        }
        #endregion

        #region IBaseQueryBuild

        public QueryContainer Equals(string key, string value)
        {
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.Bool(d => d.Must(ff => ff.Term(key, value)));
            return container;
        }

        public QueryContainer DoesNotEqual(string key, string value)
        {
            var notDescriptor = this.Equals(key, value);

            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.Bool(d => d.MustNot(ff => notDescriptor));
            return container;
        }

        public QueryContainer Contains(string key, string value)
        {
            value = $"*{value}*";
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.QueryString(d => d.Query($"{key}:{value}"));
            return container;
        }

        public QueryContainer GreaterThan(string key, string value)
        {
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            QueryContainer container = null;
        
            DateTime dateTimeValue = DateTime.MinValue;

            if (double.TryParse(value, out double doubleValue))
            {
                container = queryDescriptor.Range(b => b.GreaterThanOrEquals(doubleValue).Field(key));
            }
            if (DateTime.TryParse(value, out dateTimeValue))
            {
                container = queryDescriptor.DateRange(d => d.GreaterThanOrEquals(dateTimeValue).Field(key));
            }
            return container;
        }

        public QueryContainer LessThan(string key, string value)
        {
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            QueryContainer container = null;

            if (double.TryParse(value, out double doubleValue))
            {
                container = queryDescriptor.Bool(b => b.Filter(ff => ff.Range(d => d.LessThanOrEquals(doubleValue).Field(key))));
            }
            if (DateTime.TryParse(value, out DateTime dateTimeValue))
            {
                container = queryDescriptor.Bool(b => b.Filter(ff => ff.DateRange(d => d.LessThanOrEquals(dateTimeValue).Field(key))));
            }
            return container;
        }

        public QueryContainer StartsWith(string key, string value)
        {
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.Prefix(d => d.Field($"{key}").Value(value));
            return container;
        }

        public QueryContainer EndsWith(string key, string value)
        {
            value = $".*{value}";
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.Regexp(d => d.Value(value).Field(key).Flags("ANYSTRING"));
            return container;
        }

        public QueryContainer DoesNotContain(string key, string value)
        {
            var notDescriptor = this.Contains(key, value);
            QueryContainerDescriptor<TEntity> queryDescriptor = new QueryContainerDescriptor<TEntity>();
            var container = queryDescriptor.Bool(d => d.MustNot(ff => notDescriptor));
            return container;
        }
        #endregion

        #region private methods
        private void SetDelegateExecutor()
        {
            if (queryBuilder == null)
            {
                queryBuilder = new Dictionary<Operations, Func<string, string, QueryContainer>>();
                queryBuilder.Add(Operations.Equals, this.Equals);
                queryBuilder.Add(Operations.Contains, this.Contains);
                queryBuilder.Add(Operations.DoesNotContain, this.DoesNotContain);
                queryBuilder.Add(Operations.DoesNotEqual, this.DoesNotEqual);
                queryBuilder.Add(Operations.EndsWith, this.EndsWith);
                queryBuilder.Add(Operations.GreaterThan, this.GreaterThan);
                queryBuilder.Add(Operations.LessThan, this.LessThan);
                queryBuilder.Add(Operations.StartsWith, this.StartsWith);
            }
        }

        private QueryContainer GetQueryContainer(SearchQueryItem queryItem)
        {
            QueryContainer container = null;
            if (queryItem.Filters != null && queryItem.Filters.Count() > 0)
            {
                queryItem.Filters.ForEach((filter) =>
                {
                    var currentContainer = queryBuilder[filter.Operation].Invoke(filter.Name, filter.Value);
                    if (filter.Condition == Conditions.AND)
                        container = container == null ? currentContainer : container && currentContainer;
                    else
                        container = container == null ? currentContainer : container || currentContainer;
                });
            }
            this.AddFulltextQuery(ref container, queryItem.Fulltext);

            return container;
        }

        private AggregationContainerDescriptor<TEntity> GetAggregation(List<AggregationItem> aggregations)
        {
            AggregationContainerDescriptor<TEntity> aggregation = new AggregationContainerDescriptor<TEntity>();
            if (aggregations != null)
            {
                foreach (var item in aggregations)
                {
                    string name = item.Name;
                    switch (item.Type)
                    {
                        case AggregationExpressionType.Count:
                            aggregation.Terms(name, aggr =>
                            {
                                aggr.Field(item.Name)
                                    .Size(item.Size)
                                    //.Exclude(item.ExcludedItems)
                                    .Aggregations(child => GetAggregation(item.Children));

                                if (!string.IsNullOrEmpty(item.SortField))
                                {
                                    aggr.Order(new TermsOrder()
                                    {
                                        Key = item.SortField,
                                        Order = item.SortType == "asc" ? SortOrder.Ascending : SortOrder.Descending
                                    });
                                }
                                return aggr;
                            });
                            break;
                        case AggregationExpressionType.Range:
                            SetRangeAggregationContainer(item, aggregation);
                            break;
                        case AggregationExpressionType.Sum:
                            aggregation.Sum(name, d => d.Field(item.Name));
                            break;
                        case AggregationExpressionType.Avg:
                            aggregation.Average(name, d => d.Field(item.Name));
                            break;
                        case AggregationExpressionType.Min:
                            aggregation.Min(name, d => d.Field(item.Name));
                            break;
                        case AggregationExpressionType.Max:
                            aggregation.Max(name, d => d.Field(item.Name));
                            break;
                        default:
                            break;
                    }
                }
            }
            return aggregation;
        }

        private void AddFulltextQuery(ref QueryContainer container, string fulltext)
        {
            if (!string.IsNullOrEmpty(fulltext))
            {
                string value = $"{fulltext}";
                QueryContainerDescriptor<TEntity> queryContainerDescriptor = new QueryContainerDescriptor<TEntity>();
                var query = queryContainerDescriptor.QueryString(c => c.Query(value));
                container = container != null ? container & query : query;
            }
        }

        private void SetRangeAggregationContainer(AggregationItem currentItem, AggregationContainerDescriptor<TEntity> aggregation)
        {
            string name = currentItem.Name.Split('.').LastOrDefault();
            if (currentItem.RangesDate != null && currentItem.RangesDate.Count() > 0)
            {
                IList<IDateRangeExpression> dataRanges = new List<IDateRangeExpression>();
                foreach (var item in currentItem.RangesDate)
                {
                    var range = new DateRangeExpression() { Key = currentItem.Name };
                    range.From = item.From;
                    range.To = item.To;
                    dataRanges.Add(range);
                }
                aggregation.DateRange(name, d => d.Field(currentItem.Name).Ranges(dataRanges.ToArray()));
            }
            else if (currentItem.Ranges != null && currentItem.Ranges.Count() > 0)
            {
                List<Func<RangeDescriptor, IRange>> funcs = new List<Func<RangeDescriptor, IRange>>();
                foreach (var item in currentItem.Ranges)
                {
                    funcs.Add(d =>
                    {
                        d.From(item.From)
                         .To(item.To)
                         .Key(currentItem.Name);

                        return d;
                    });
                }
                aggregation.Range(name, f => f.Field(currentItem.Name).Ranges(funcs.ToArray()));
            }
        }

        #endregion
    }
}
