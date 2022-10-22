namespace MultiMillionaire.Models.Lifelines;

public class AskTheAudience : Lifeline
{
    public enum State
    {
        Setup,
        InProgress,
        ResultsReveal
    }

    public State CurrentState { get; private set; } = State.Setup;
    public bool UseAi { get; set; }

    private Dictionary<char, int> Votes { get; } = new();

    private TaskCompletionSource AllPlayersAnsweredSignal { get; } = new();
    private int AudienceSize { get; set; }

    public async Task StartVotingAndWait(int audienceSize)
    {
        CurrentState = State.InProgress;
        AudienceSize = audienceSize;

        // Timeout after 20 seconds if nobody answers
        await Task.WhenAny(Task.Delay(20000), AllPlayersAnsweredSignal.Task);

        CurrentState = State.ResultsReveal;
    }

    public void SubmitVote(char letter)
    {
        if (Votes.ContainsKey(letter))
            Votes[letter]++;
        else
            Votes.Add(letter, 1);
        CheckAllAudienceAnswered();
    }

    private void CheckAllAudienceAnswered()
    {
        if (TotalVotes() == AudienceSize)
            AllPlayersAnsweredSignal.SetResult();
    }

    public void GenerateAiResponses()
    {
        Votes.Add('A', 10);
        Votes.Add('B', 20);
        Votes.Add('C', 28);
        Votes.Add('D', 3);
        CurrentState = State.ResultsReveal;
    }

    private int TotalVotes()
    {
        return Votes.Sum(kvp => kvp.Value);
    }

    public Dictionary<char, int> GetPercentages()
    {
        var totalVotes = TotalVotes();
        var answerCount = Votes.Count;

        // Use integer division to get value <= 100
        var percentages = Votes.ToDictionary(kvp => kvp.Key, kvp => 100 * kvp.Value / totalVotes);
        var difference = 100 - percentages.Sum(kvp => kvp.Value);

        // Distribute remaining % points arbitrarily between answers
        while (difference > 0)
        {
            var index = difference % answerCount;
            var key = percentages.ElementAt(index).Key;
            percentages[key]++;
            difference--;
        }

        return percentages;
    }
}