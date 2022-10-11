namespace MultiMillionaire.Models;

public class MillionaireRound : GameRound
{
    public User? Player { get; init; }
    public QuestionBank Questions { get; set; } = new();
    public int QuestionNumber { get; set; } = 1;
    public bool Locked { get; set; } = true;

    public int GetBackgroundNumber()
    {
        return QuestionNumber switch
        {
            <= 5 => 1,
            <= 10 => 2,
            _ => 3
        };
    }

    public MultipleChoiceQuestion GetCurrentQuestion()
    {
        return Questions.GetQuestion(QuestionNumber);
    }

    public static string GetValueFromQuestionNumber(int? questionNumber)
    {
        return questionNumber switch
        {
            1 => "£100",
            2 => "£200",
            3 => "£300",
            4 => "£500",
            5 => "£1,000",
            6 => "£2,000",
            7 => "£4,000",
            8 => "£8,000",
            9 => "£16,000",
            10 => "£32,000",
            11 => "£64,000",
            12 => "£125,000",
            13 => "£250,000",
            14 => "£500,000",
            15 => "£1 MILLION",
            _ => ""
        };
    }
}