using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace ElasticConnector.IntegrationTest.Models
{
    [ElasticsearchType(IdProperty = "Id")]
    public class Company
    {
        public int Id { get; set; }

        [Text(Index = false)]
        public string Name { get; set; }

        [Text]
        public string Address { get; set; }

        [Text(Analyzer ="keyword")]
        public string Country { get; set; }

        [Text]
        public string City { get; set; }

        [Text(Index = false)]
        public string MainEmailAddress { get; set; }
    }
}
