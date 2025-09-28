using Domain;

namespace TestDomain;

[TestClass]
public class AttractionTest
{
    [TestMethod]
    public void Attraction_ShouldHaveRequiredProperties()
    {
        Attraction attraction = new Attraction();
        
        attraction.id = Guid.NewGuid();
        attraction.name = "Race simulator";
        attraction.description = "average race simulator";
        attraction.type = AttractionType.Simulator;
        attraction.minAge = 18;
        attraction.maxCapacity = 10;
        attraction.currentCapacity = 0;
        attraction.isActive = true;
        
        Assert.IsNotNull(attraction.id);
        Assert.AreEqual("Race simulator", attraction.name);
        Assert.AreEqual("average race simulator", attraction.description);
        Assert.AreEqual(AttractionType.Simulator, attraction.type);
        Assert.AreEqual(18, attraction.minAge);
        Assert.AreEqual(10, attraction.maxCapacity);
        Assert.AreEqual(0, attraction.currentCapacity);
        Assert.AreEqual(true, attraction.isActive);
    }
}