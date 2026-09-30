using System.Text.Json;
using MenuDB;
using MenuDB.Data;

namespace MenuApi.Outbox;

public class OutboxWriter(MenuDbContext db)
{
    public void Write<TEvent>(TEvent domainEvent)
        where TEvent : notnull
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        db.OutboxEvents.Add(new OutboxEvent
        {
            Id = Guid.CreateVersion7(),
            EventType = typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            CreatedAtUtc = DateTime.UtcNow,
        });
    }
}
