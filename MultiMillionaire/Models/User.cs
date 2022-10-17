namespace MultiMillionaire.Models;

public class User
{
    public User(string connectionId)
    {
        ConnectionId = connectionId;
    }

    // TEMP
    public static int PlayerNum { get; set; } = 2;

    public string ConnectionId { get; set; }
    public string? Name { get; set; }
    public MultiplayerGame? Game { get; set; }
    public UserRole? Role { get; set; }

    private string GetScore()
    {
        if (Game == null || !Game.Scores.ContainsKey(this)) return "";
        return MillionaireRound.FormatValueAsString(Game.Scores[this]);
    }

    public UserViewModel ToViewModel()
    {
        return new UserViewModel
        {
            ConnectionId = ConnectionId,
            Name = Name,
            Role = Role,
            Score = GetScore()
        };
    }
}

public enum UserRole
{
    Host,
    Audience,
    Spectator
}