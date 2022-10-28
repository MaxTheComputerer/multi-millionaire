using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MultiMillionaire.Database;

public class OrderQuestionDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Question { get; set; } = null!;
    public Dictionary<char, string> Answers { get; set; } = null!;
    public List<char> CorrectOrder { get; set; } = null!;
    public string? Comment { get; set; }
}