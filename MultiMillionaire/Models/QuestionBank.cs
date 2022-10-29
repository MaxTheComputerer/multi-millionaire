using MultiMillionaire.Models.Questions;
using MultiMillionaire.Models.Rounds;
using MultiMillionaire.Services;

namespace MultiMillionaire.Models;

public class QuestionBank
{
    private List<MultipleChoiceQuestion> Questions { get; } = new();

    public MultipleChoiceQuestion GetQuestion(int questionNumber)
    {
        return Questions[questionNumber - 1];
    }

    public static async Task<QuestionBank> GenerateQuestionBank(IDatabaseService databaseService,
        HashSet<string> excludedIds)
    {
        var bank = new QuestionBank();
        for (var i = 1; i <= 15; i++)
        {
            var difficulty = MillionaireRound.GetDifficultyFromQuestionNumber(i);
            var question = await databaseService.GetMultipleChoiceQuestionExcept(difficulty, excludedIds);

            bank.Questions.Add(MultipleChoiceQuestion.FromDbModel(question));
            if (question.Id != null) excludedIds.Add(question.Id!);
        }

        return bank;
    }
}