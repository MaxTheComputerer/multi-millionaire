namespace MultiMillionaire.Models.Lifelines;

public class Lifeline
{
    public bool IsUsed { get; set; }

    public static ConfidenceLevel ParseConfidenceString(string value)
    {
        return value switch
        {
            "certain" => ConfidenceLevel.Certain,
            "prettySure" => ConfidenceLevel.PrettySure,
            "fiftyFifty" => ConfidenceLevel.FiftyFifty,
            _ => ConfidenceLevel.Guess
        };
    }
}