using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ElasticsearchConnect.Api.Infrastructure
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public static IDictionary<Type, Action<ExceptionContext>> container;

        public CustomExceptionFilterAttribute()
        {
            this.Register();
        }
        public override void OnException(ExceptionContext context)
        {
            Action<ExceptionContext> action = null;
            if (container.TryGetValue(context.Exception.GetType(), out action))
            {
                action.Invoke(context);
            }
            base.OnException(context);
        }

        private void Register()
        {
            if (container == null)
            {
                container = new Dictionary<Type, Action<ExceptionContext>>();
                container.Add(typeof(ArgumentNullException), this.ArgumentNullExceptionResponse);
            }
        }

        private void ArgumentNullExceptionResponse(ExceptionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            JsonErrorResponseModel response = new JsonErrorResponseModel()
            {
                ErrorCode = (int)HttpStatusCode.BadRequest,
                Message = context.Exception.Message
            };
            context.Result = new BadRequestObjectResult(response);
            //LOG
        }
    }
}
