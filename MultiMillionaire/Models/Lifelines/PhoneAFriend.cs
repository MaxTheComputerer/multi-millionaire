using Newtonsoft.Json;
using Weighted_Randomizer;

namespace MultiMillionaire.Models.Lifelines;

public class PhoneAFriend : Lifeline
{
    private static readonly Random _rnd = new();

    static PhoneAFriend()
    {
        using var r = new StreamReader("data/phone_responses.json");
        var json = r.ReadToEnd();
        Responses = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)!.ToDictionary(
            kvp => ParseConfidenceString(kvp.Key), kvp => kvp.Value);
    }

    public bool InProgress { get; set; }
    public bool UseAi { get; set; }
    private ConfidenceLevel? Confidence { get; set; }

    private static Dictionary<ConfidenceLevel, List<string>> Responses { get; }


    public IEnumerable<char> ChooseLetters(List<char> remainingAnswers, char correctAnswer)
    {
        GenerateConfidenceLevel(remainingAnswers.Count == 2);
        var randomizer = new StaticWeightedRandomizer<char>();

        // Each answer starts off being equally probable
        foreach (var letter in remainingAnswers) randomizer[letter] = 1;

        // Change the weight of the correct answer depending on our confidence
        randomizer[correctAnswer] = Confidence switch
        {
            ConfidenceLevel.Certain => 1000,
            ConfidenceLevel.PrettySure or ConfidenceLevel.FiftyFifty => 97,
            _ => randomizer[correctAnswer]
        };

        // Choose answers. First element is what we think is most likely to be the answer
        var guesses = new List<char> { randomizer.NextWithRemoval(), randomizer.NextWithRemoval() };

        // Shuffle if 5050
        if (Confidence == ConfidenceLevel.FiftyFifty) guesses.Shuffle(_rnd);
        return guesses;
    }

    private void GenerateConfidenceLevel(bool usedFiftyFifty)
    {
        var randomizer = new StaticWeightedRandomizer<ConfidenceLevel>();
        if (usedFiftyFifty)
        {
            randomizer[ConfidenceLevel.Certain] = 4;
            randomizer[ConfidenceLevel.PrettySure] = 5;
            randomizer[ConfidenceLevel.Guess] = 1;
        }
        else
        {
            randomizer[ConfidenceLevel.Certain] = 20;
            randomizer[ConfidenceLevel.PrettySure] = 50;
            randomizer[ConfidenceLevel.FiftyFifty] = 25;
            randomizer[ConfidenceLevel.Guess] = 5;
        }

        Confidence = randomizer.NextWithReplacement();
    }

    public string GenerateResponse(List<string> answers)
    {
        var responses = Responses[Confidence!.Value];
        var chosenResponse = responses.ChooseRandom(_rnd);
        return chosenResponse
            .Replace("ANS1", answers.FirstOrDefault())
            .Replace("ANS2", answers.LastOrDefault());
    }
}