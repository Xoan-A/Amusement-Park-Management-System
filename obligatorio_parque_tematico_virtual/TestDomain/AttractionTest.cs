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

        Assert.AreEqual("Race simulator", attraction.Name);
        Assert.AreEqual(AttractionType.Simulator, attraction.Type);
        Assert.AreEqual(18, attraction.MinAge);
    }

    [TestMethod]
    public void Attraction_ShouldHaveUniqueId()
    {
        Attraction attraction1 = new Attraction();
        Attraction attraction2 = new Attraction();

        Assert.AreNotEqual(attraction1.Id, attraction2.Id);
    }

    [TestMethod]
    public void Attraction_Incidents_ShouldSaveCorrectly()
    {
        Attraction attraction1 = new Attraction();
        attraction1.AddIncident("Incident 1");

        Assert.AreEqual("Incident 1", attraction1.Incidents[0]);


        attraction1.RemoveIncident("Incident 1");
        Assert.AreEqual(0, attraction1.Incidents.Count);
    }
}