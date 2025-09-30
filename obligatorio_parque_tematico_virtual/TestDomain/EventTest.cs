using System.Runtime.InteropServices.JavaScript;
using Domain;

namespace TestDomain;

[TestClass]
public class EventTest
{
    [TestMethod]
    public void Event_ShouldHaveRequiredProperties()
    {
        Event event = new Event();
        
        event.Id = Guid.NewGuid();
        event.Name = "Music Festival";
        event.Date = new DateTime(2024, 8, 15);
        event.Hour = 10;
        event.MaxCapacity = 5000;
        event.CurrentCapacity = 0;
        event.Cost = 100;
        event.Attractions = new List<Attraction>();
        
        Assert.IsNotNull(event.Id);
        Assert.AreEqual("Music Festival", event.Name);
        Assert.AreEqual(new DateTime(2024, 8, 15), event.Date);
        Assert.AreEqual(10, event.Hour);
        Assert.AreEqual(5000, event.MaxCapacity);
        Assert.AreEqual(0, event.CurrentCapacity);
        Assert.AreEqual(100, event.Cost);
        Assert.IsNotNull(event.Attractions);
    }
}