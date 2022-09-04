namespace MultiMillionaire.Models;

public class MultiplayerGame
{
    public string Id { get; set; } = string.Empty;
    public User Host { get; set; } = null!;
    public List<User> Audience { get; set; } = new();
    public List<User> Spectators { get; set; } = new();

    public static string GenerateRoomId()
    {
        return Guid.NewGuid().ToString()[..5].ToUpper();
    }
}