using System.Text.Json;
using MenuDB;
using MenuDB.Data;

namespace MenuApi.Outbox;

public class OutboxWriter(MenuDbContext db)
{
    public void Write<TEvent>(TEvent domainEvent, Guid? eventId = null)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        db.OutboxEvents.Add(new OutboxEvent
        {
            Id = eventId ?? Guid.CreateVersion7(),
            EventType = typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            CreatedAtUtc = DateTime.UtcNow,
        });
    }
}
