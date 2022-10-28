using Lifx;
using MultiMillionaire.Database;
using MultiMillionaire.Models.Questions;
using MultiMillionaire.Models.Rounds;
using MultiMillionaire.Services;

namespace MultiMillionaire.Models;

public class MultiplayerGame
{
    public string Id { get; init; } = string.Empty;
    public GameSettings Settings { get; } = new();
    public User Host { get; init; } = null!;
    public List<User> Audience { get; } = new();
    public List<User> Spectators { get; } = new();
    public Dictionary<User, int> Scores { get; } = new();
    public GameRound? Round { get; private set; }
    public User? NextPlayer { get; set; }
    public ILight? Light { get; set; }
    private HashSet<string> UsedOrderQuestionIds { get; } = new();

    public static readonly List<char> AnswerLetters = new() { 'A', 'B', 'C', 'D' };
    private readonly IDatabaseService _databaseService;

    public MultiplayerGame(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

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

    public List<User> GetListeners()
    {
        List<User> listeners = new();
        if (!Settings.MuteHostSound) listeners.Add(Host);
        if (!Settings.MuteAudienceSound) listeners.AddRange(Audience);
        if (!Settings.MuteSpectatorSound) listeners.AddRange(Spectators);
        return listeners;
    }

    public bool IsReadyForNewRound()
    {
        return Round == null && Audience.Count > 0;
    }

    public async Task<FastestFingerFirst> SetupFastestFingerRound()
    {
        OrderQuestionDbModel questionDbModel;
        try
        {
            questionDbModel = await _databaseService.GetRandomOrderQuestionExcept(UsedOrderQuestionIds);
        }
        catch (InvalidOperationException)
        {
            UsedOrderQuestionIds.Clear();
            questionDbModel = await _databaseService.GetRandomOrderQuestionExcept(UsedOrderQuestionIds);
        }

        if (questionDbModel.Id != null)
            UsedOrderQuestionIds.Add(questionDbModel.Id);

        Round = new FastestFingerFirst
        {
            Players = GetNotPlayedPlayers().ToList(),
            Question = OrderQuestion.FromDbModel(questionDbModel)
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

    public async Task ConnectToLight()
    {
        DisconnectFromLight();

        var lightFactory = new LightFactory();
        Light = await lightFactory.CreateLightAsync(Settings.LifxLightIp);
    }

    public void DisconnectFromLight()
    {
        Light?.Dispose();
        Light = null;
    }
}