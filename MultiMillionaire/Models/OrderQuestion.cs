namespace MultiMillionaire.Models;

public class OrderQuestion
{
    public string Question { get; init; } = string.Empty;
    public Dictionary<char, string> Answers { get; } = new();
    public List<char> CorrectOrder { get; } = new();

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
