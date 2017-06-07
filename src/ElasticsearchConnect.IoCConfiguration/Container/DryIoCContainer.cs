using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfiguration
{
    public class DryIoCContainer : IInstanceContainer
    {
        Container currentContainer;

        public DryIoCContainer()
        {
            currentContainer = IoCFactory.GetServiceIoCContainer<Container>();
        }

        public T Resolve<T>()
        {
            return currentContainer.Resolve<T>();
        }

        public T Resolve<T>(string name)
        {
            var result = this.currentContainer.Resolve<T>(serviceKey: name);
            return result;
        }

        public List<T> ResolveMany<T>()
        {
            return currentContainer.ResolveMany<T>().ToList();
        }

        public T Resolve<T>(string scope, string source)
        {
            string name = string.Format("{0}{1}{2}", typeof(T).Name, scope, source);
            return this.Resolve<T>(name);
        }

        public T ResolveByScope<T>(string scope)
        {
            string name = string.Format("{0}{1}", typeof(T).Name, scope);
            return this.Resolve<T>(name);
        }

        public List<T> ResolveManyByScope<T>(string region)
        {
            List<T> result = new List<T>();
            using(var scope = this.currentContainer.OpenScope(region))
            {
                result = scope.ResolveMany<T>().ToList();
            }
            return result;
        }
    }
}
