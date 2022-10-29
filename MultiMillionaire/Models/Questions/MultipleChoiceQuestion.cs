using MultiMillionaire.Database;

namespace MultiMillionaire.Models.Questions;

public enum QuestionDifficulty
{
    FirstTwo,
    SectionOne,
    SectionTwo,
    SectionThree,
    FinalQuestion
}

public class MultipleChoiceQuestion : QuestionBase
{
    public char CorrectLetter { get; private init; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.SectionOne;

    public static MultipleChoiceQuestion FromDbModel(MultipleChoiceQuestionDbModel dbModel)
    {
        return new MultipleChoiceQuestion
        {
            Question = dbModel.Question,
            Answers = dbModel.Answers,
            CorrectLetter = dbModel.CorrectLetter,
            Difficulty = (QuestionDifficulty)dbModel.Difficulty,
            Comment = dbModel.Comment
        };
    }
}