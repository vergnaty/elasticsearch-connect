using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api
{
    public class JsonErrorResponseModel
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
