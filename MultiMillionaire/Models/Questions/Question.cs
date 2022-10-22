namespace MultiMillionaire.Models.Questions;

public abstract class QuestionBase
{
    public string Question { get; protected init; } = string.Empty;
    public Dictionary<char, string> Answers { get; } = new();
}