namespace MultiMillionaire.Models;

public class FastestFingerFirst : GameRound
{
    public List<User> Players { get; init; } = new();
    public OrderQuestion? Question { get; init; }
    public Dictionary<User, double> Times { get; } = new();
    public Dictionary<User, bool> GaveCorrectAnswer { get; } = new();
    public bool InProgress { get; set; }
    public CancellationTokenSource AllPlayersAnsweredToken { get; set; } = new();

    public async Task StartRoundAndWait()
    {
        InProgress = true;
        // Timeout after 20 seconds if nobody answers
        await Task.Delay(20000, AllPlayersAnsweredToken.Token);
        InProgress = false;
    }

    public List<string> GetPlayerIds()
    {
        return Players.Select(u => u.ConnectionId).ToList();
    }

    private void CheckAllPlayersAnswered()
    {
        if (Times.Count == Players.Count && GaveCorrectAnswer.Count == Players.Count) AllPlayersAnsweredToken.Cancel();
    }

    public void SubmitAnswer(User player, IEnumerable<char> answerOrder, double time)
    {
        Times.Add(player, time);
        GaveCorrectAnswer.Add(player, answerOrder.Equals(Question!.CorrectOrder));
        CheckAllPlayersAnswered();
    }

    public static OrderQuestion GenerateQuestion()
    {
        return new OrderQuestion
        {
            Question = "Starting with the lowest number, put the answers to these sums in numerical order.",
            Answers =
            {
                ['A'] = "9 + 3",
                ['B'] = "9 - 3",
                ['C'] = "9 x 3",
                ['D'] = "9 ÷ 3"
            },
            CorrectOrder = { 'D', 'B', 'A', 'C' }
        };
    }
}