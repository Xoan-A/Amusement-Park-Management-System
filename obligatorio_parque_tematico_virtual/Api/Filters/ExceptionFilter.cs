using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is KeyNotFoundException)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 404
            };
        }
        else if (context.Exception is ArgumentException)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 400
            };
        }
        else if (context.Exception is NotImplementedException)
        {
            context.Result = new ObjectResult(new { Message = "Not implemented" })
            {
                StatusCode = 501
            };
        }
        else
        {
            context.Result = new ObjectResult(new { Message = "An unexpected error occurred" })
            {
                StatusCode = 500
            };
        }
    }
}