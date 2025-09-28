using Domain;

namespace TestDomain;

[TestClass]
public class AttractionTest
{
    [TestMethod]
    public void Attraction_ShouldHaveRequiredProperties()
    {
        Attraction attraction = new Attraction();
        
        attraction.Id = Guid.NewGuid();
        attraction.Name = "Race simulator";
        attraction.Description = "average race simulator";
        attraction.Type = AttractionType.Simulator;
        attraction.MinAge = 18;
        attraction.MaxCapacity = 10;
        attraction.CurrentCapacity = 0;
        attraction.IsActive = true;
        
        Assert.IsNotNull(attraction.Id);
        Assert.AreEqual("Race simulator", attraction.Name);
        Assert.AreEqual("average race simulator", attraction.Description);
        Assert.AreEqual(AttractionType.Simulator, attraction.Type);
        Assert.AreEqual(18, attraction.MinAge);
        Assert.AreEqual(10, attraction.MaxCapacity);
        Assert.AreEqual(0, attraction.CurrentCapacity);
        Assert.AreEqual(true, attraction.IsActive);
    }

    [TestMethod]
    public void Attraction_ShouldHaveUniqueId()
    {
        Attraction attraction1 = new Attraction();
        Attraction attraction2 = new Attraction();
        
        Assert.AreNotEqual(attraction1.Id, attraction2.Id);
    }
}