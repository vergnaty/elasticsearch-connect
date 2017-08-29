using ElasticConnector.Model;
using System.Collections.Generic;


namespace ElasticConnector.Repository
{
    public interface ISearchEngine<TEntity> 
        where TEntity : class
    {
        /// <summary>
        /// Save a document 
        /// </summary>
        /// <param name="entity">The Entity.</param>
        void Save(TEntity entity);

        /// <summary>
        /// Save Many Documents using Bulk operation
        /// </summary>
        /// <param name="entites"></param>
        void SaveMany(IEnumerable<TEntity> entites);

        /// <summary>
        /// Save Many Documents using Bulk operation _Parent/_Child
        /// </summary>
        /// <param name="entites">the entities with spesific partent key</param>
        void SaveMany(Dictionary<object,TEntity> entites);

        /// <summary>
        /// Delete documents by Query
        /// </summary>
        /// <param name="query">the query</param>
        /// <returns>return either successful or failed</returns>
        bool DeleteByQuery(params CustomFilter[] query);

        /// <summary>
        /// Fulltext Search use Custom query builder
        /// </summary>
        /// <param name="queryItem">the elastic search query item. </param>
        /// </param>
        /// <returns>SearchResult of <see cref="TEntity"/></returns>
        SearchResult<TEntity> FullTextSearch(SearchQueryItem queryItem);

        /// <summary>
        /// Create a new Index.
        /// </summary>
        /// <param name="indexName">the Index name.</param>
        void CreateIndex(string indexName);

        /// <summary>
        /// Create a Completation Suggest Index
        /// </summary>
        /// <param name="indexName">the index name.</param>
        void CreateCompletationSuggestIndex(string indexName);

        //SearchSuggestResult Autocomplate(string q, BaseSearchSuggestFilter baseFilter);

        /// <summary>
        /// Drop index by Name
        /// </summary>
        /// <param name="indexName">The index name.</param>
        void DropIndex(string indexName);

        /// <summary>
        /// Swap Alias
        /// <remark>Use change alias in order to avoid downtime</remark>
        /// </summary>
        /// <param name="oldIndex">the old index name</param>
        /// <param name="newIndex">The new index name</param>
        void SwapAlias(string oldIndex, string newIndex);
        
        /// <summary>
        /// Get all fields for the specific index and type
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<string, string>> GetFilterFields();

        /// <summary>
        /// Set the index type
        /// </summary>
        /// <param name="name">the name</param>
        ISearchEngine<TEntity> SetType(string name);

        /// <summary>
        /// Set the Index Name
        /// </summary>
        /// <param name="name"></param>
        ISearchEngine<TEntity> SetIndexName(string name);

        /// <summary>
        /// Set Alias Name 
        /// this could have multiple indexs associated with the same Alias
        /// </summary>
        /// <param name="name"></param>
        ISearchEngine<TEntity> SetAlias(string name);
    }
}
