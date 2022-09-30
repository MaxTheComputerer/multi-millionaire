namespace MultiMillionaire.Models;

public class OrderQuestion
{
    public string Question { get; set; } = string.Empty;
    public Dictionary<char, string> Answers { get; set; } = new();
    public List<char> CorrectOrder { get; set; } = new();
}