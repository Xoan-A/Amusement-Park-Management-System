using Microsoft.VisualStudio.TestTools.UnitTesting;
using Domain;

namespace TestDomain;

[TestClass]
public class PluginInfoTest
{
    [TestMethod]
    public void Name_ValidValue_SetsSuccessfully()
    {
        PluginInfo plugin = new PluginInfo();

        plugin.Name = "TestPlugin";

        Assert.AreEqual("TestPlugin", plugin.Name);
    }

    [TestMethod]
    public void Name_EmptyString_ThrowsArgumentException()
    {
        PluginInfo plugin = new PluginInfo();

        Assert.ThrowsException<ArgumentException>(() => { plugin.Name = ""; });
    }

    [TestMethod]
    public void Name_WhitespaceOnly_ThrowsArgumentException()
    {
        PluginInfo plugin = new PluginInfo();

        Assert.ThrowsException<ArgumentException>(() => { plugin.Name = "   "; });
    }

    [TestMethod]
    public void Name_Null_ThrowsArgumentException()
    {
        PluginInfo plugin = new PluginInfo();

        Assert.ThrowsException<ArgumentException>(() => { plugin.Name = null!; });
    }


    [TestMethod]
    public void AllProperties_ValidValues_SetSuccessfully()
    {
        PluginInfo plugin = new PluginInfo
        {
            Name = "TestStrategy",
            AssemblyPath = "/path/to/plugin.dll",
            TypeName = "TestNamespace.TestStrategy"
        };

        Assert.AreEqual("TestStrategy", plugin.Name);
        Assert.AreEqual("/path/to/plugin.dll", plugin.AssemblyPath);
        Assert.AreEqual("TestNamespace.TestStrategy", plugin.TypeName);
    }
}