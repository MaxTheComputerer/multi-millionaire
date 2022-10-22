using Weighted_Randomizer;

namespace MultiMillionaire.Models.Lifelines;

public class LifelineAi
{
    public static IEnumerable<char> ChooseLetters(List<char> remainingAnswers, char correctAnswer,
        ConfidenceLevel confidence, Random rnd)
    {
        var randomizer = new StaticWeightedRandomizer<char>();

        // Each answer starts off being equally probable
        foreach (var letter in remainingAnswers) randomizer[letter] = 1;

        // Change the weight of the correct answer depending on our confidence
        randomizer[correctAnswer] = confidence switch
        {
            ConfidenceLevel.Certain => 1000,
            ConfidenceLevel.PrettySure or ConfidenceLevel.FiftyFifty => 97,
            _ => randomizer[correctAnswer]
        };

        // Choose answers. First element is what we think is most likely to be the answer
        var guesses = new List<char> { randomizer.NextWithRemoval(), randomizer.NextWithRemoval() };

        // Shuffle if 5050
        if (confidence == ConfidenceLevel.FiftyFifty) guesses.Shuffle(rnd);
        return guesses;
    }

    public static ConfidenceLevel GenerateConfidenceLevel(bool usedFiftyFifty)
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

        return randomizer.NextWithReplacement();
    }
}