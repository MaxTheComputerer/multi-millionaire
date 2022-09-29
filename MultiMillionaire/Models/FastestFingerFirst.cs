namespace MultiMillionaire.Models;

public class FastestFingerFirst : GameRound
{
    public List<User> Players { get; set; } = new();
    public OrderQuestion? Question { get; set; }

    public static async Task<OrderQuestion> GenerateQuestion()
    {
        return new OrderQuestion();
    }
}