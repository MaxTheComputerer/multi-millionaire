namespace MultiMillionaire.Models;

public class FastestFingerFirst : GameRound
{
    public List<User> Players { get; init; } = new();
    public OrderQuestion? Question { get; init; }
    public Dictionary<User, double> Times { get; } = new();
    public Dictionary<User, bool> GaveCorrectAnswer { get; } = new();
    public RoundState State { get; set; } = RoundState.Setup;
    private TaskCompletionSource AllPlayersAnsweredSignal { get; } = new();
    public int AnswerRevealIndex { get; set; }

    public enum RoundState
    {
        Setup,
        InProgress,
        AnswerReveal,
        ResultsReveal
    }

    public async Task StartRoundAndWait()
    {
        State = RoundState.InProgress;
        // Timeout after 20 seconds if nobody answers
        await Task.WhenAny(Task.Delay(20000), AllPlayersAnsweredSignal.Task);
        State = RoundState.AnswerReveal;
    }

    public List<string> GetPlayerIds()
    {
        return Players.Select(u => u.ConnectionId).ToList();
    }

    private void CheckAllPlayersAnswered()
    {
        if (Times.Count == Players.Count && GaveCorrectAnswer.Count == Players.Count)
            AllPlayersAnsweredSignal.SetResult();
    }

    public void SubmitAnswer(User player, IEnumerable<char> answerOrder, double time)
    {
        Times.Add(player, Math.Round(time, 2));
        GaveCorrectAnswer.Add(player, answerOrder.SequenceEqual(Question!.CorrectOrder));
        CheckAllPlayersAnswered();
    }

    public char GetNextAnswer()
    {
        var correctOrder = Question!.CorrectOrder;
        return correctOrder[AnswerRevealIndex++];
    }
}