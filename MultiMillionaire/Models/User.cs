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

    public string GetScore()
    {
        return MillionaireRound.GetValueFromQuestionNumber(Game?.Scores.GetValueOrDefault(this));
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