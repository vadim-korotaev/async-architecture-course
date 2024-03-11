namespace Common.Events;

public class TaskCompleted : Event
{
    public TaskCompleted(Guid taskPublicId)
    {
        TaskPublicId = taskPublicId;
    }
    
    public Guid TaskPublicId { get; }
}