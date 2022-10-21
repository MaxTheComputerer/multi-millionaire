namespace MultiMillionaire.Models.Lifelines;

public class AskTheAudience : Lifeline
{
    public enum State
    {
        Setup,
        InProgress,
        ResultsReveal
    }

    public State CurrentState { get; set; } = State.Setup;
    public bool UseAi { get; set; }

    public Dictionary<char, int> Votes { get; set; } = new()
    {
        ['A'] = 0,
        ['B'] = 0,
        ['C'] = 0,
        ['D'] = 0
    };

    private TaskCompletionSource AllPlayersAnsweredSignal { get; } = new();
    public int AudienceSize { get; set; }

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
        if (!Votes.ContainsKey(letter)) return;
        Votes[letter]++;
        CheckAllAudienceAnswered();
    }

    private void CheckAllAudienceAnswered()
    {
        if (Votes.Sum(kvp => kvp.Value) == AudienceSize)
            AllPlayersAnsweredSignal.SetResult();
    }

    public void GenerateAiResponses()
    {
        Votes['A'] = 10;
        Votes['B'] = 20;
        Votes['C'] = 28;
        Votes['D'] = 3;
        CurrentState = State.ResultsReveal;
    }
}