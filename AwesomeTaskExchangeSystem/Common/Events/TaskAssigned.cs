namespace Common.Events;

public class TaskAssigned : Event
{
    public TaskAssigned (Guid taskPublicId, Guid assignedUserPublicId)
    {
        TaskPublicId = taskPublicId;
        AssignedUserPublicId = assignedUserPublicId;
    }
    
    public Guid TaskPublicId { get; }
    
    public Guid AssignedUserPublicId { get; }
}