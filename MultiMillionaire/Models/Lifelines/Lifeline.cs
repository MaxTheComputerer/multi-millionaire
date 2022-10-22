namespace MultiMillionaire.Models.Lifelines;

public abstract class Lifeline
{
    public bool IsUsed { get; set; }

    protected static ConfidenceLevel ParseConfidenceString(string value)
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