namespace MultiMillionaire.Models.Lifelines;

public class FiftyFifty : Lifeline
{
    public List<char> RemovedAnswers { get; set; } = new();
    private static readonly Random _rnd = new();

    public IEnumerable<char> RemoveAnswersFromQuestion(MultipleChoiceQuestion question)
    {
        var correctLetter = question.CorrectLetter;
        var letters = new List<char> { 'A', 'B', 'C', 'D' };
        letters.Remove(correctLetter);
        RemovedAnswers = letters.OrderBy(l => _rnd.Next()).Take(2).ToList();
        return RemovedAnswers;
    }

    public void Reset()
    {
        RemovedAnswers.Clear();
    }

    public bool IsAnswerRemoved(char letter)
    {
        return RemovedAnswers.Contains(letter);
    }
}