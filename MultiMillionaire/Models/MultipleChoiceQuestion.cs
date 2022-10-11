namespace MultiMillionaire.Models;

public class MultipleChoiceQuestion
{
    public string Question { get; init; } = string.Empty;
    public Dictionary<char, string> Answers { get; } = new();
    public char CorrectLetter { get; set; }

    public static MultipleChoiceQuestion GenerateQuestion(int questionNumber)
    {
        return new MultipleChoiceQuestion
        {
            Question = "What is the name of the system of transmitting messages between computer terminals?",
            Answers =
            {
                ['A'] = "B-Mail",
                ['B'] = "C-Mail",
                ['C'] = "E-Mail",
                ['D'] = "D-Mail"
            },
            CorrectLetter = 'C'
        };
    }
}