namespace MenuDB.Data;

public class OutboxEvent
{
    public Guid Id { get; set; }

    public required string EventType { get; set; }

    public required string Payload { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }
}
