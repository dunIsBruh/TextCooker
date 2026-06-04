namespace text_cooker.Entities;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public Role Role { get; set; } = Role.User;
    public DateTime CreatedAt { get; set; }
}

public enum Role
{
    Admin,
    User,
    Anonymous
}