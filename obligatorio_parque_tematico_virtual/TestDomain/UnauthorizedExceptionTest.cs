using Domain.Exceptions;

namespace TestDomain;

[TestClass]
public class UnauthorizedExceptionTest
{
    [TestMethod]
    public void Constructor_WithNoArgs_SetsDefaultMessage()
    {
        var exception = new UnauthorizedException();

        Assert.AreEqual("Unauthorized access", exception.Message);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_SetsCustomMessage()
    {
        string customMessage = "Custom unauthorized message";
        var exception = new UnauthorizedException(customMessage);

        Assert.AreEqual(customMessage, exception.Message);
    }
}
