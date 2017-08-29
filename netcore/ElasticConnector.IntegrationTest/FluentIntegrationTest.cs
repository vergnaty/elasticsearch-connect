using ElasticConnector.Model;
using ElasticConnector.Repository;
using System;
using System.IO;

namespace ElasticConnector.IntegrationTest
{
    public class FluentIntegrationTest : ITestDataSetup, ITestProcess, IDisposable
    {
        private ISearchEngine<object> searchEngine;

        #region properties
        public object CustomResult { get; private set; }

        public dynamic[] Data { get; private set; }

        public string IndexName { get; private set; }

        public SearchResult<object> SearchReponse { get; private set; }

        #endregion

        public static ITestProcess Create()
        {
            string[] urls = new string[] { "http://localhost:9200" };
            // var container = IoCFactory.GetContainer();
            var instance = new FluentIntegrationTest();
            instance.searchEngine = new ElasticsearchSearchEngine<object>(urls);
            return instance;
        }

        #region interface Implementations

        public ITestProcess Assert(Action<object> action)
        {
            using (this)
            {
                action(this.CustomResult);
                return this;
            }
        }

        public ITestProcess Assert(Action<dynamic[], SearchResult<object>> action)
        {
            using (this)
            {
                action.Invoke(this.Data, this.SearchReponse);
                return this;
            }
        }

        public ITestProcess Assert(Action<SearchResult<object>> action)
        {
            using (this)
            {
                action.Invoke(this.SearchReponse);
                return this;
            }
        }

        public ITestProcess Assert(Action action)
        {
            using (this)
            {
                action.Invoke();
                return this;
            }
        }

        public void Cleanup()
        {
           this.searchEngine.DropIndex(this.IndexName);
        }

        public ITestProcess ExecuteCustom(Func<ISearchEngine<object>, object> action)
        {
            this.CustomResult = action.Invoke(this.searchEngine);
            return this;
        }

        public ITestProcess ExecuteSearch(SearchQueryItem query)
        {
            this.SearchReponse = this.searchEngine.FullTextSearch(query);
            return this;
        }

        public ITestProcess LoadData(string path)
        {
            string json = File.ReadAllText(path);

            //Deserialize test Data
            this.Data = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(json);
            return this;
        }

        public ITestProcess LoadData(Func<dynamic[]> action)
        {
            this.Data = action.Invoke();
            if (this.Data != null)
                this.searchEngine.SaveMany(this.Data);
            return this;
        }

        public ITestDataSetup Start(string indexName)
        {
            this.IndexName = indexName;
            this.searchEngine.CreateIndex(indexName);
            return this;
        }

        public void Dispose()
        {
            this.Cleanup();
            this.searchEngine = null;
        }
        #endregion
    }

    #region interfaces
    public interface ITestDataSetup
    {
        /// <summary>
        /// Get the data loaded
        /// </summary>
        dynamic[] Data { get; }
        /// <summary>
        /// Load data by executing an action
        /// </summary>
        /// <param name="action">the delegate.</param>
        ITestProcess LoadData(Func<dynamic[]> action);
        /// <summary>
        /// Load data from file system
        /// </summary>
        /// <param name="path">the path</param>
        ITestProcess LoadData(string path);
    }

    public interface ITestProcess
    {
        SearchResult<object> SearchReponse { get; }

        object CustomResult { get; }

        /// <summary>
        /// the index name
        /// </summary>
        string IndexName { get; }

        /// <summary>
        /// Create index
        /// </summary>
        /// <returns></returns>
        ITestDataSetup Start(string indexName);

        /// <summary>
        /// Execute search
        /// </summary>
        /// <param name="action"></param>
        ITestProcess ExecuteCustom(Func<ISearchEngine<object>, object> action);

        /// <summary>
        /// Execute search
        /// </summary>
        ITestProcess ExecuteSearch(SearchQueryItem query);

        /// <summary>
        /// Assert actions
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        ITestProcess Assert(Action<SearchResult<object>> action);

        /// <summary>
        /// Assert actions with custom result
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        ITestProcess Assert(Action<object> action);

        /// <summary>
        /// Assert data between Loaded Data and Search Response 
        /// </summary>
        /// <param name="action">the action</param>
        /// <returns></returns>
        ITestProcess Assert(Action<dynamic[], SearchResult<object>> action);

        /// <summary>
        /// Clean data and Drop index
        /// </summary>
        void Cleanup();

    }
    #endregion
}
