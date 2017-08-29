using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticConnector.Model
{
    public static class BaseExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items,Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
