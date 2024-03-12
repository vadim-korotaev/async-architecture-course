namespace Common;

public class UserDto
{
    public Guid PublicId { get; set; }
    
    public string Username { get; set; }
    
    public UserRole Role { get; set; }
    
    public bool IsDeleted { get; set; }
}