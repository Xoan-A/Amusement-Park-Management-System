using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is NotImplementedException)
        {
            context.Result = new ObjectResult(new { Message = "Not implemented" })
            {
                StatusCode = 501
            };
        }
        else if (context.Exception is Exception)
        {
            context.Result = new ObjectResult(new { Message = "An unexpected error occurred" })
            {
                StatusCode = 500
            };
        }
    }
}