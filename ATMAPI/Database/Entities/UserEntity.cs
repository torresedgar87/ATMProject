namespace ATMAPI.Database.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public Guid IdAlias { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
}