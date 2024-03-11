namespace Common.Events;

public class StreamingEvent<T> : Event
{
    public StreamingEvent(T data) : base()
    {
        Data = data;
    }
    
    public T Data { get; }
}