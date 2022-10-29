namespace MultiMillionaire.Database;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string OrderQuestionsCollectionName { get; set; } = null!;
    public string MultipleChoiceQuestionsCollectionName { get; set; } = null!;
}