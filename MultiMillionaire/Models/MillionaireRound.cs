namespace MultiMillionaire.Models;

public class MillionaireRound : GameRound
{
    public User? Player { get; init; }
    public QuestionBank QuestionBank { get; set; } = new();
    public int QuestionNumber { get; set; } = 1;
    public char? SubmittedAnswer { get; set; }
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
        return QuestionBank.GetQuestion(QuestionNumber);
    }

    public string GetWinnings()
    {
        return GetValueFromQuestionNumber(QuestionNumber);
    }

    public void FinishQuestion()
    {
        SubmittedAnswer = null;
        QuestionNumber++;
    }

    public int GetQuestionsAway()
    {
        return 16 - QuestionNumber;
    }

    public string GetUnsafeAmount()
    {
        return QuestionNumber switch
        {
            1 => "£0",
            2 => "£100",
            3 => "£200",
            4 => "£300",
            5 => "£500",
            6 => "£0",
            7 => "£1,000",
            8 => "£3,000",
            9 => "£7,000",
            10 => "£15,000",
            11 => "£0",
            12 => "£32,000",
            13 => "£93,000",
            14 => "£218,000",
            15 => "£468,000",
            _ => ""
        };
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