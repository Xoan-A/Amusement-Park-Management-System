using Domain.Exceptions;

namespace TestDomain;

[TestClass]
public class UnauthorizedExceptionTest
{
    [TestMethod]
    public void Constructor_WithNoArgs_SetsDefaultMessage()
    {
        UnauthorizedException exception = new UnauthorizedException();

        Assert.AreEqual("Unauthorized access", exception.Message);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_SetsCustomMessage()
    {
        string customMessage = "Custom unauthorized message";
        UnauthorizedException exception = new UnauthorizedException(customMessage);

        Assert.AreEqual(customMessage, exception.Message);
    }
}
