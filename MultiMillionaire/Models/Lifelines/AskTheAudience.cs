using MultiMillionaire.Models.Questions;
using Weighted_Randomizer;

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
        AddVoteToDictionary(letter);
        CheckAllAudienceAnswered();
    }

    private void AddVoteToDictionary(char letter)
    {
        if (Votes.ContainsKey(letter))
            Votes[letter]++;
        else
            Votes.Add(letter, 1);
    }

    private void CheckAllAudienceAnswered()
    {
        if (TotalVotes() == AudienceSize)
            AllPlayersAnsweredSignal.SetResult();
    }

    public void GenerateAiResponses(MultipleChoiceQuestion question, List<char> remainingAnswers)
    {
        // Generate audience's confidence
        var confidence = LifelineAi.GenerateConfidenceLevel(remainingAnswers.Count == 2);

        // Set up voting weights according to confidence
        var randomizer = new StaticWeightedRandomizer<char>();
        foreach (var letter in remainingAnswers) randomizer[letter] = 1;
        var chosenLetters = LifelineAi.ChooseLetters(remainingAnswers, question.CorrectLetter, confidence);
        switch (confidence)
        {
            case ConfidenceLevel.Certain:
                randomizer[question.CorrectLetter] = 10;
                break;
            case ConfidenceLevel.PrettySure:
                randomizer[chosenLetters.First()] = 20;
                randomizer[chosenLetters.Last()] = 10;
                break;
            case ConfidenceLevel.FiftyFifty:
                randomizer[chosenLetters.First()] = 10;
                randomizer[chosenLetters.Last()] = 10;
                break;
            case ConfidenceLevel.Guess:
            default:
                break;
        }

        // Cast votes
        for (var i = 0; i < 200; i++)
        {
            var letter = randomizer.NextWithReplacement();
            AddVoteToDictionary(letter);
        }

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