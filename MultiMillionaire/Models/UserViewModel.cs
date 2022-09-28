namespace MultiMillionaire.Models;

public class UserViewModel
{
    public string ConnectionId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public UserRole? Role { get; set; }
}