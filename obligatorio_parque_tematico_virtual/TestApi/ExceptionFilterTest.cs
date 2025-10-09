using System.Net;
using Api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace ApiTests;

[TestClass]
public class ExceptionFilterTest
{
    private ExceptionContext _context;
    private ExceptionFilter _attribute = new ExceptionFilter();

    [TestInitialize]
    public void Initialize()
    {
        _context = new ExceptionContext(
            new ActionContext(
                new Mock<HttpContext>(MockBehavior.Strict).Object,
                new RouteData(),
                new ActionDescriptor()),
            new List<IFilterMetadata>());
    }

    [TestMethod]
    public void OnException_WhenExceptionIsNotRegistered_ShouldResponseInternalError()
    {
        _context.Exception = new Exception("Internal server error");
        _attribute.OnException(_context);

        IActionResult? response = _context.Result;
        ObjectResult? concreteResponse = response as ObjectResult;


        Assert.AreEqual((int)HttpStatusCode.InternalServerError, concreteResponse.StatusCode);
        Assert.AreEqual("500", concreteResponse.StatusCode.ToString());
        Assert.AreEqual("Internal server error", GetMessage(concreteResponse.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsNotImplemented_ShouldResponseNotImplemented()
    {
        _context.Exception = new NotImplementedException();
        _attribute.OnException(_context);

        IActionResult? response = _context.Result;
        ObjectResult? concreteResponse = response as ObjectResult;

        Assert.AreEqual((int)HttpStatusCode.NotImplemented, concreteResponse.StatusCode);
        Assert.AreEqual("501", concreteResponse.StatusCode.ToString());
        Assert.AreEqual("The method or operation is not implemented.", GetMessage(concreteResponse.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsKeyNotFound_ShouldResponseNotFound()
    {
        _context.Exception = new KeyNotFoundException("Attraction not found");
        _attribute.OnException(_context);

        ObjectResult? response = _context.Result as ObjectResult;
        Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("404", response.StatusCode.ToString());
        Assert.AreEqual("Attraction not found", GetMessage(response.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsArgument_ShouldResponseBadRequest()
    {
        _context.Exception = new ArgumentException("Invalid data");
        _attribute.OnException(_context);

        ObjectResult? response = _context.Result as ObjectResult;
        Assert.AreEqual((int)HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("400", response.StatusCode.ToString());
        Assert.AreEqual("Invalid data", GetMessage(response.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsUnauthorized_ShouldResponse401()
    {
        _context.Exception = new Domain.Exceptions.UnauthorizedException("Unauthorized access");
        _attribute.OnException(_context);

        ObjectResult? response = _context.Result as ObjectResult;
        Assert.AreEqual((int)HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual("401", response.StatusCode.ToString());
        Assert.AreEqual("Unauthorized access", GetMessage(response.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsForbidden_ShouldResponse403()
    {
        _context.Exception = new Domain.Exceptions.ForbiddenException("Forbidden access");
        _attribute.OnException(_context);

        ObjectResult? response = _context.Result as ObjectResult;
        Assert.AreEqual((int)HttpStatusCode.Forbidden, response.StatusCode);
        Assert.AreEqual("403", response.StatusCode.ToString());
        Assert.AreEqual("Forbidden access", GetMessage(response.Value));
    }

    private string GetMessage(object value) =>
        value?.GetType().GetProperty("Message")?.GetValue(value)?.ToString() ?? string.Empty;
}