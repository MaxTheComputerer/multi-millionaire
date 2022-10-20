using MultiMillionaire.Models.Lifelines;

namespace MultiMillionaire.Models;

public class MillionaireRound : GameRound
{
    public User? Player { get; init; }
    public QuestionBank QuestionBank { get; set; } = new();
    public int QuestionNumber { get; set; } = 1;
    public char? SubmittedAnswer { get; set; }
    public bool Locked { get; set; } = true;
    public bool HasWalkedAway { get; set; }

    public FiftyFifty FiftyFifty { get; set; } = new();
    public PhoneAFriend PhoneAFriend { get; set; } = new();
    public bool UsedAskTheAudience { get; set; }

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
        return GetValueStringFromQuestionNumber(QuestionNumber);
    }

    public void FinishQuestion()
    {
        SubmittedAnswer = null;
        QuestionNumber++;
        FiftyFifty.Reset();
    }

    public int GetQuestionsAway()
    {
        return 16 - QuestionNumber;
    }

    public int GetTotalPrize()
    {
        if (HasWalkedAway) return GetValueFromQuestionNumber(QuestionNumber - 1);
        return QuestionNumber switch
        {
            > 10 => 32000,
            > 5 => 1000,
            _ => 0
        };
    }

    public string GetTotalPrizeString()
    {
        return FormatValueAsString(GetTotalPrize());
    }

    public IEnumerable<char> GetFiftyFiftyAnswers()
    {
        FiftyFifty.IsUsed = true;
        return FiftyFifty.RemoveAnswersFromQuestion(GetCurrentQuestion());
    }

    public void StartPhoneAFriend()
    {
        PhoneAFriend.IsUsed = true;
        PhoneAFriend.InProgress = true;
    }

    public string GeneratePhoneAiResponse()
    {
        var question = GetCurrentQuestion();
        var remainingAnswers = new List<char> { 'A', 'B', 'C', 'D' }.Where(l => !FiftyFifty.RemovedAnswers.Contains(l))
            .ToList();
        var chosenLetters = PhoneAFriend.ChooseLetters(remainingAnswers, question.CorrectLetter);
        var chosenAnswers = chosenLetters.Select(l => question.Answers[l]).ToList();
        return PhoneAFriend.GenerateResponse(chosenAnswers);
    }

    public void EndPhoneAFriend()
    {
        PhoneAFriend.InProgress = false;
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

    public static string GetValueStringFromQuestionNumber(int? questionNumber)
    {
        return FormatValueAsString(GetValueFromQuestionNumber(questionNumber));
    }

    public static string FormatValueAsString(int? value)
    {
        return value switch
        {
            0 => "£0",
            100 => "£100",
            200 => "£200",
            300 => "£300",
            500 => "£500",
            1000 => "£1,000",
            2000 => "£2,000",
            4000 => "£4,000",
            8000 => "£8,000",
            16000 => "£16,000",
            32000 => "£32,000",
            64000 => "£64,000",
            125000 => "£125,000",
            250000 => "£250,000",
            500000 => "£500,000",
            1000000 => "£1 MILLION",
            _ => ""
        };
    }

    public static int GetValueFromQuestionNumber(int? questionNumber)
    {
        return questionNumber switch
        {
            1 => 100,
            2 => 200,
            3 => 300,
            4 => 500,
            5 => 1000,
            6 => 2000,
            7 => 4000,
            8 => 8000,
            9 => 16000,
            10 => 32000,
            11 => 64000,
            12 => 125000,
            13 => 250000,
            14 => 500000,
            15 => 1000000,
            _ => 0
        };
    }
}