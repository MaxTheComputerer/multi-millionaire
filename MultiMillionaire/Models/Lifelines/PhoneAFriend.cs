using MultiMillionaire.Models.Questions;
using Newtonsoft.Json;

namespace MultiMillionaire.Models.Lifelines;

public class PhoneAFriend : Lifeline
{
    private static Dictionary<ConfidenceLevel, List<string>> Responses { get; }

    static PhoneAFriend()
    {
        using var r = new StreamReader("data/phone_responses.json");
        var json = r.ReadToEnd();
        Responses = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)!.ToDictionary(
            kvp => ParseConfidenceString(kvp.Key), kvp => kvp.Value);
    }

    public bool InProgress { get; set; }
    public bool UseAi { get; set; }

    public string GenerateAiResponse(MultipleChoiceQuestion question, List<char> remainingAnswers)
    {
        var confidence = LifelineAi.GenerateConfidenceLevel(remainingAnswers.Count == 2);
        var chosenLetters = LifelineAi.ChooseLetters(remainingAnswers, question.CorrectLetter, confidence);
        var chosenAnswers = chosenLetters.Select(l => question.Answers[l]).ToList();

        var responses = Responses[confidence];
        var chosenResponse = responses.ChooseRandom(Random.Shared);
        return chosenResponse
            .Replace("ANS1", chosenAnswers.FirstOrDefault())
            .Replace("ANS2", chosenAnswers.LastOrDefault());
    }
}