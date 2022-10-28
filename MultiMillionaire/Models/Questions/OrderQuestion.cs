using MultiMillionaire.Database;

namespace MultiMillionaire.Models.Questions;

public class OrderQuestion : QuestionBase
{
    public List<char> CorrectOrder { get; private init; } = new();
    public string? Comment { get; set; }

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

    public static OrderQuestion FromDbModel(OrderQuestionDbModel dbModel)
    {
        return new OrderQuestion
        {
            Question = dbModel.Question,
            Answers = dbModel.Answers,
            CorrectOrder = dbModel.CorrectOrder,
            Comment = dbModel.Comment
        };
    }
}