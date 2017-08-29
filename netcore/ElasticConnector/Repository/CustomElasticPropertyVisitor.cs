using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ElasticConnector.Repository
{
    public class CustomElasticPropertyVisitor : NoopPropertyVisitor
    {
        public override void Visit(ITextProperty type, PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
        {
            if (type.Type.ToString() == "string" & type.Index != null && type.Index.Value)
            {
                var properties = new Properties();
                properties.Add("sort", new TextProperty { Analyzer = "case_insensitive_sort" });
                type.Fields = properties;
            }
            base.Visit(type, propertyInfo, attribute);
        }

        public static Dictionary<string, string> GetStringSotableField<TEntity>()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            Func<PropertyInfo, string> getValue = d => d.PropertyType == typeof(string) ? d.Name + ".sort" : d.Name;
            typeof(TEntity).GetProperties()
                          .ToList().ForEach(a => results.Add(a.Name, getValue(a)));

            return results;
        }
    }
}
