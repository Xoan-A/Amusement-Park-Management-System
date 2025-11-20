using IBusinessLogic.Exceptions;

namespace TestBusinessLogic;

[TestClass]
public class ForbiddenExceptionTest
{
    [TestMethod]
    public void Constructor_WithNoArgs_SetsDefaultMessage()
    {
        ForbiddenException exception = new ForbiddenException();

        Assert.AreEqual("Forbidden access", exception.Message);
    }

    [TestMethod]
    public void Constructor_WithCustomMessage_SetsCustomMessage()
    {
        string customMessage = "Custom forbidden message";
        ForbiddenException exception = new ForbiddenException(customMessage);

        Assert.AreEqual(customMessage, exception.Message);
    }
}
