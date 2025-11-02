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
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 401
            };
        }
        else if (context.Exception is ForbiddenException)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 403
            };
        }
        else if (context.Exception is KeyNotFoundException)
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
        else if (context.Exception is InvalidOperationException)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 400
            };
        }
        else if (context.Exception is NotImplementedException)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 501
            };
        }
        else
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message })
            {
                StatusCode = 500
            };
        }
    }
}