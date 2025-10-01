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
    private ExceptionFilter _attribute;

    public ExceptionFilterTest()
    {
        _attribute = new ExceptionFilter();
    }

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
        _context.Exception = new Exception("Not registered");
        _attribute.OnException(_context);

        var response = _context.Result;
        var concreteResponse = response as ObjectResult;


        Assert.AreEqual((int)HttpStatusCode.InternalServerError, concreteResponse.StatusCode);
        Assert.AreEqual("500", concreteResponse.StatusCode.ToString());
        Assert.AreEqual("An unexpected error occurred", GetMessage(concreteResponse.Value));
    }

    [TestMethod]
    public void OnException_WhenExceptionIsNotImplemented_ShouldResponseNotImplemented()
    {
        _context.Exception = new NotImplementedException();
        _attribute.OnException(_context);

        var response = _context.Result;
        var concreteResponse = response as ObjectResult;

        Assert.AreEqual((int)HttpStatusCode.NotImplemented, concreteResponse.StatusCode);
        Assert.AreEqual("501", concreteResponse.StatusCode.ToString());
        Assert.AreEqual("Not implemented", GetMessage(concreteResponse.Value));
    }

    private string GetMessage(object value) =>
        value.GetType().GetProperty("Message").GetValue(value).ToString();
}