using MultiMillionaire.Models.Rounds;

namespace MultiMillionaire.Models;

public class User
{
    public User(string connectionId, bool isMobile = false)
    {
        ConnectionId = connectionId;
        IsMobile = isMobile;
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string ConnectionId { get; set; }
    public string? Name { get; set; }
    public MultiplayerGame? Game { get; set; }
    public UserRole? Role { get; set; }
    public bool IsMobile { get; }

    private string GetScore()
    {
        if (Game == null || !Game.Scores.ContainsKey(this)) return "";
        return MillionaireRound.FormatValueAsString(Game.Scores[this]);
    }

    public UserViewModel ToViewModel()
    {
        return new UserViewModel
        {
            Id = Id.ToString(),
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