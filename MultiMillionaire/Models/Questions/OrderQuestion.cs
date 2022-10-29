using MultiMillionaire.Database;

namespace MultiMillionaire.Models.Questions;

public class OrderQuestion : QuestionBase
{
    public List<char> CorrectOrder { get; private init; } = new();

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