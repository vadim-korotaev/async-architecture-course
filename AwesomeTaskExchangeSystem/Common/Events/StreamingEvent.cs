namespace Common.Events;

public class StreamingEvent<T> : Event
{
    public StreamingEvent(string eventName, T data) : base()
    {
        EventName = eventName;
        Data = data;
    }
    
    public string EventName { get; }
    
    public T Data { get; }
}