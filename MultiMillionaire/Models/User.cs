namespace MultiMillionaire.Models;

public class User
{
    public string ConnectionId { get; set; }
    public string? Name { get; set; }
    public MultiplayerGame? Game { get; set; }

    public User(string connectionId)
    {
        ConnectionId = connectionId;
    }
}

public enum UserRole
{
    Host,
    Audience,
    Spectator
}