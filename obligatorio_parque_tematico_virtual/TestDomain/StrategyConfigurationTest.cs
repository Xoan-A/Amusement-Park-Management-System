using Domain;

namespace TestDomain;

[TestClass]
public class StrategyConfigurationTest
{
    [TestMethod]
    public void StrategyConfiguration_ShouldHaveRequiredProperties()
    {
        StrategyConfiguration config = new StrategyConfiguration();

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
        StrategyConfiguration config = new StrategyConfiguration();

        Assert.AreEqual(string.Empty, config.StrategyName);
    }

    [TestMethod]
    public void StrategyConfiguration_ShouldAllowPerAttractionStrategy()
    {
        StrategyConfiguration config = new StrategyConfiguration
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
        StrategyConfiguration config = new StrategyConfiguration
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
        StrategyConfiguration config = new StrategyConfiguration
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
        StrategyConfiguration config = new StrategyConfiguration
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
        StrategyConfiguration config = new StrategyConfiguration
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
        StrategyConfiguration config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = "Combo",
            N = 30,
        };

        config.N = null;

        Assert.IsNull(config.N);
    }
}
