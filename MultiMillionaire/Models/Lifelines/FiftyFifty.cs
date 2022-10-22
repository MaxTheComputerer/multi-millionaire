using MultiMillionaire.Models.Questions;

namespace MultiMillionaire.Models.Lifelines;

public class FiftyFifty : Lifeline
{
    public List<char> RemovedAnswers { get; private set; } = new();

    public IEnumerable<char> RemoveAnswersFromQuestion(MultipleChoiceQuestion question)
    {
        var correctLetter = question.CorrectLetter;
        var letters = new List<char>(MultiplayerGame.AnswerLetters);
        letters.Remove(correctLetter);
        letters.Shuffle(Random.Shared);
        RemovedAnswers = letters.Take(2).ToList();
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