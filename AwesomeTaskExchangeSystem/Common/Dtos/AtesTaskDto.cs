namespace Common;

public class AtesTaskDto
{
    public Guid PublicId { get; set; }
    
    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    
    public AtesTaskStatus Status { get; set; }
    
    public Guid AssignedUser { get; set; }
}