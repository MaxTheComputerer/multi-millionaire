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

    private IEnumerable<User> GetNotPlayedPlayers()
    {
        return Audience.FindAll(u => !Scores.ContainsKey(u));
    }

    public bool IsReadyForNewRound()
    {
        return Round == null && Audience.Count > 0;
    }

    public FastestFingerFirst SetupFastestFingerRound()
    {
        Round = new FastestFingerFirst
        {
            Players = GetNotPlayedPlayers().ToList(),
            Question = OrderQuestion.GenerateQuestion()
        };
        return (FastestFingerFirst)Round;
    }

    public void SetupMillionaireRound()
    {
        Round = new MillionaireRound
        {
            Player = NextPlayer,
            QuestionBank = QuestionBank.GenerateQuestionBank()
        };
    }

    public void ResetRound()
    {
        Round = null;
    }

    public void SaveScore(User player, int score)
    {
        Scores[player] = score;
    }

    public void RemoveUser(User user)
    {
        Audience.Remove(user);
        Spectators.Remove(user);
        Scores.Remove(user);
        if (NextPlayer == user) NextPlayer = null;
    }
}