using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain;

namespace TestDomain;

[TestClass]
public class PluginInfoTest
{
    [TestMethod]
    public void Name_ValidValue_SetsSuccessfully()
    {
        // Arrange
        var plugin = new PluginInfo();

        // Act
        plugin.Name = "TestPlugin";

        // Assert
        Assert.AreEqual("TestPlugin", plugin.Name);
    }

    [TestMethod]
    public void Name_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var plugin = new PluginInfo();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            plugin.Name = "";
        });
    }

    [TestMethod]
    public void Name_WhitespaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var plugin = new PluginInfo();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            plugin.Name = "   ";
        });
    }

    [TestMethod]
    public void Name_Null_ThrowsArgumentException()
    {
        // Arrange
        var plugin = new PluginInfo();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            plugin.Name = null!;
        });
    }

    [TestMethod]
    public void Description_NullValue_SetsToEmptyString()
    {
        // Arrange
        var plugin = new PluginInfo();

        // Act
        plugin.Description = null!;

        // Assert
        Assert.AreEqual(string.Empty, plugin.Description);
    }

    [TestMethod]
    public void AllProperties_ValidValues_SetSuccessfully()
    {
        // Arrange & Act
        var plugin = new PluginInfo
        {
            Name = "TestStrategy",
            Description = "Test description",
            Author = "Test Author",
            Version = "1.0.0",
            AssemblyPath = "/path/to/plugin.dll",
            TypeName = "TestNamespace.TestStrategy"
        };

        // Assert
        Assert.AreEqual("TestStrategy", plugin.Name);
        Assert.AreEqual("Test description", plugin.Description);
        Assert.AreEqual("Test Author", plugin.Author);
        Assert.AreEqual("1.0.0", plugin.Version);
        Assert.AreEqual("/path/to/plugin.dll", plugin.AssemblyPath);
        Assert.AreEqual("TestNamespace.TestStrategy", plugin.TypeName);
    }
}
