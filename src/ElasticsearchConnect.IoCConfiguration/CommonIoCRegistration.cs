using ElasticsearchConnect.Api.IoCConfiguration;
using ElasticsearchConnect.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfigurator
{
    public class CommonIoCRegistration
    {
        public static void Register()
        {
            var container = IoCFactory.GetIoCRegistration();
            
            string[] urls = new string[] { "http://localhost:9200" };
            container.Instance<ISearchEngine<object>, ElasticsearchSearchEngine<object>>(new ElasticsearchSearchEngine<object>(urls));
        }
    }
}
