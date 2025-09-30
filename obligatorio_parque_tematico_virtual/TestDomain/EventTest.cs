using System.Runtime.InteropServices.JavaScript;
using Domain;

namespace TestDomain;

[TestClass]
public class EventTest
{
    [TestMethod]
    public void Event_ShouldHaveRequiredProperties()
    {
        Event newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Music Festival",
            Date = new DateTime(2024, 8, 15),
            Hour = 10,
            MaxCapacity = 5000,
            CurrentCapacity = 0,
            Cost = 100,
            Attractions = []
        };

        Assert.IsNotNull(newEvent.Id);
        Assert.AreEqual("Music Festival", newEvent.Name);
        Assert.AreEqual(new DateTime(2024, 8, 15), newEvent.Date);
        Assert.AreEqual(10, newEvent.Hour);
        Assert.AreEqual(5000, newEvent.MaxCapacity);
        Assert.AreEqual(0, newEvent.CurrentCapacity);
        Assert.AreEqual(100, newEvent.Cost);
        Assert.IsNotNull(newEvent.Attractions);
    }

    [TestMethod]
    public void Event_ShouldHaveUniqueId()
    {
        Event event1 = new Event();
        Event event2 = new Event();
        
        Assert.AreNotEqual(event1.Id, event2.Id);
    }
}