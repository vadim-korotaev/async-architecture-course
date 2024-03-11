namespace Common.Events;

public class UserLogged : Event
{
    public UserLogged(Guid userPublicId) : base()
    {
        UserPublicId = userPublicId;
    }
    
    public Guid UserPublicId { get; }
}