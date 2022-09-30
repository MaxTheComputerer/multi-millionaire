namespace MultiMillionaire.Models;

public class FastestFingerFirst : GameRound
{
    public List<User> Players { get; set; } = new();
    public OrderQuestion? Question { get; set; }

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