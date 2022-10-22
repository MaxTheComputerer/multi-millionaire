namespace MultiMillionaire.Models.Questions;

public class MultipleChoiceQuestion : QuestionBase
{
    public char CorrectLetter { get; private init; }

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