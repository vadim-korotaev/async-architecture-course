namespace Common.Events;

public abstract class Event
{
    public Guid Id { get; } = Guid.NewGuid();
    public long Timestamp { get; } = DateTime.UtcNow.Ticks;
}