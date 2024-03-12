namespace Common;

public class AtesTaskDto
{
    public int Id { get; set; }
    
    public Guid PublicId { get; set; }
    
    public string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    
    public AtesTaskStatus Status { get; set; }
    
    public Guid Assigned { get; set; }
}