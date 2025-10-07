using Domain;

namespace TestDomain;

[TestClass]
public class StrategyConfigurationTest
{
    [TestMethod]
    public void StrategyConfiguration_ShouldHaveRequiredProperties()
    {
        var config = new StrategyConfiguration();

        config.Id = 1;
        config.StrategyName = "PerAttraction";
        config.N = null;

        Assert.AreEqual(1, config.Id);
        Assert.AreEqual("PerAttraction", config.StrategyName);
        Assert.IsNull(config.N);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldSetStrategyNameToEmptyStringByDefault()
    {
        var config = new StrategyConfiguration();

        Assert.AreEqual(string.Empty, config.StrategyName);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowPerAttractionStrategy()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "PerAttraction",
            N = null,
        };

        Assert.AreEqual("PerAttraction", config.StrategyName);
        Assert.IsNull(config.N);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowPerEventStrategy()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "PerEvent",
            N = null,
        };

        Assert.AreEqual("PerEvent", config.StrategyName);
        Assert.IsNull(config.N);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowComboStrategyWithN()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "Combo",
            N = 30,
        };

        Assert.AreEqual("Combo", config.StrategyName);
        Assert.AreEqual(30, config.N);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowUpdatingStrategyName()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "PerAttraction",
        };

        config.StrategyName = "PerEvent";

        Assert.AreEqual("PerEvent", config.StrategyName);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowUpdatingNValue()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "Combo",
            N = 30,
        };

        config.N = 45;

        Assert.AreEqual(45, config.N);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowSettingNToNull()
    {
        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "Combo",
            N = 30,
        };

        config.N = null;

        Assert.IsNull(config.N);
    }
}
