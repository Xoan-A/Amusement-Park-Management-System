using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Domain.Exceptions;

namespace Api.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedException)
        {
            context.Result = new ObjectResult(new { Message = "Required privileges not met" })
            {
                StatusCode = 401
            };
        }
        else if (context.Exception is ForbiddenException)
        {
            context.Result = new ObjectResult(new { Message = "An authorization is mandatory for this request" })
            {
                StatusCode = 403
            };
        }
        else if (context.Exception is KeyNotFoundException)
        {
            context.Result = new ObjectResult(new { Message = "Key not found in server" })
            {
                StatusCode = 404
            };
        }
        else if (context.Exception is ArgumentException)
        {
            context.Result = new ObjectResult(new { Message = "Incorrect request data" })
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
            context.Result = new ObjectResult(new { Message = "Internal server error" })
            {
                StatusCode = 500
            };
        }
    }
}