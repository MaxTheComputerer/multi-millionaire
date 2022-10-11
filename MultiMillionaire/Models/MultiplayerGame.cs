namespace MultiMillionaire.Models;

public class MultiplayerGame
{
    public string Id { get; set; } = string.Empty;
    public GameSettings Settings { get; set; } = new();
    public User Host { get; set; } = null!;
    public List<User> Audience { get; set; } = new();
    public List<User> Spectators { get; set; } = new();
    public Dictionary<User, int> Scores { get; set; } = new();
    public GameRound? Round { get; set; }
    public User? NextPlayer { get; set; }

    public static string GenerateRoomId()
    {
        return Guid.NewGuid().ToString()[..5].ToUpper();
    }

    public IEnumerable<User> GetPlayers()
    {
        return Audience.OrderByDescending(u => Scores.GetValueOrDefault(u)).Prepend(Host);
    }

    public bool IsReadyForNewRound()
    {
        return Round == null && Audience.Count > 0;
    }

    private List<User> GetNotPlayedPlayers()
    {
        return Audience.FindAll(u => !Scores.ContainsKey(u)).ToList();
    }

    public void SetupFastestFingerRound()
    {
        Round = new FastestFingerFirst
        {
            Players = GetNotPlayedPlayers(),
            Question = OrderQuestion.GenerateQuestion()
        };
    }

    public void SetupMillionaireRound()
    {
        Round = new MillionaireRound
        {
            Player = NextPlayer,
            Questions = QuestionBank.GenerateQuestionBank()
        };
    }

    public void ResetRound()
    {
        Round = null;
    }
}