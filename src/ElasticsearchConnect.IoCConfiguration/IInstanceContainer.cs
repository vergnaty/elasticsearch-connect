using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.IoCConfiguration
{
    public interface IInstanceContainer
    {
        T Resolve<T>();
        T Resolve<T>(string name);
        T Resolve<T>(string scope, string source);
        T ResolveByScope<T>(string scope);
        List<T> ResolveMany<T>();
        List<T> ResolveManyByScope<T>(string scope);
    }
}
