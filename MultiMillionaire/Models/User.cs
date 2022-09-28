namespace MultiMillionaire.Models;

public class User
{
    public User(string connectionId)
    {
        ConnectionId = connectionId;
    }

    public string ConnectionId { get; set; }
    public string? Name { get; set; }
    public MultiplayerGame? Game { get; set; }
    public UserRole? Role { get; set; }

    public string GetScore()
    {
        return Question.GetValueFromQuestionNumber(Game?.Scores[this]);
    }

    public UserViewModel ToViewModel()
    {
        return new UserViewModel
        {
            ConnectionId = ConnectionId,
            Name = Name,
            Role = Role
        };
    }
}

public enum UserRole
{
    Host,
    Audience,
    Spectator
}