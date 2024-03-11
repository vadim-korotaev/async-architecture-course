namespace Common;

public class User
{
    public int Id { get; set; }
    
    public Guid PublicId { get; set; }
    
    public string Username { get; set; } = "";
    
    public string Password { get; set; } = "";
    
    public UserRole Role { get; set; } = UserRole.Worker;
    
    public bool IsDeleted { get; set; } = false;
}