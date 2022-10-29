using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MultiMillionaire.Database;

public class MultipleChoiceQuestionDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Question { get; set; } = null!;
    public Dictionary<char, string> Answers { get; set; } = null!;
    public char CorrectLetter { get; set; }
    public int Difficulty { get; set; }
    public string? Comment { get; set; }
}