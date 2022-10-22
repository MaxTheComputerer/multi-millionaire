using MultiMillionaire.Models.Questions;

namespace MultiMillionaire.Models;

public class QuestionBank
{
    private List<MultipleChoiceQuestion> Questions { get; } = new();

    public MultipleChoiceQuestion GetQuestion(int questionNumber)
    {
        return Questions[questionNumber - 1];
    }

    public static QuestionBank GenerateQuestionBank()
    {
        var bank = new QuestionBank();
        for (var i = 1; i <= 15; i++) bank.Questions.Add(MultipleChoiceQuestion.GenerateQuestion(i));
        return bank;
    }
}