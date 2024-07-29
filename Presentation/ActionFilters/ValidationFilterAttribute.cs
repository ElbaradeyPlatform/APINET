
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc.Filters;
using Entities.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.ActionFilters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        public ValidationFilterAttribute()
        { }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];
            var param = context.ActionArguments.SingleOrDefault(x => x.Value.ToString().Contains("Dto")).Value;
            if (param is null)
            {
                 var error = new GenericError($"Object is null.", "NO DATA", Guid.NewGuid(), DateTime.Now, false);
                 throw new ApiException(customError: error);
                //ontext.Result = new BadRequestObjectResult($" {error}");
                //   context.Result = new BadRequestObjectResult(new GenericResponse(Guid.NewGuid(), DateTime.Now, false, 402, "")); // BadRequestObjectResult($"Object is null. Controller:{ controller }, action: { action}");
                //    return ;
            }
            if (!context.ModelState.IsValid)
                //context.Result = new UnprocessableEntityObjectResult(context.ModelState);
                throw new ApiException(context.ModelState.AllErrors(),422);
            //throw new ApiException(context.ModelState);
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
