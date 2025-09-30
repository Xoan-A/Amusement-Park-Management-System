using Domain;

namespace TestDomain;

[TestClass]
public class EventAttractionTest
{
    [TestMethod]
    public void EventAttraction_ShouldHaveRequiredProperties()
    {
        Event newEvent = new Event();
        Attraction attraction = new Attraction();
        EventAttraction eventAttraction = new EventAttraction
        {
            EventId = newEvent.Id,
            Event = newEvent,
            AttractionId = attraction.Id,
            Attraction = attraction
        };

        Assert.AreEqual(eventAttraction.EventId, eventAttraction.Event.Id);
        Assert.AreEqual(eventAttraction.AttractionId, eventAttraction.Attraction.Id);
    }
}