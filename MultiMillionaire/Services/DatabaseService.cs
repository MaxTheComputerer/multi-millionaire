using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MultiMillionaire.Database;
using MultiMillionaire.Models.Questions;

namespace MultiMillionaire.Services;

public interface IDatabaseService
{
    public Task<OrderQuestionDbModel> GetRandomOrderQuestionExcept(IEnumerable<string> excludedIds);

    public Task<MultipleChoiceQuestionDbModel> GetMultipleChoiceQuestionExcept(QuestionDifficulty difficulty,
        IEnumerable<string> excludedIds);
}

public class DatabaseService : IDatabaseService
{
    private readonly IMongoCollection<OrderQuestionDbModel> _orderQuestionsCollection;
    private readonly IMongoCollection<MultipleChoiceQuestionDbModel> _multipleChoiceQuestionsCollection;

    public DatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

        _orderQuestionsCollection =
            mongoDatabase.GetCollection<OrderQuestionDbModel>(databaseSettings.Value.OrderQuestionsCollectionName);

        _multipleChoiceQuestionsCollection =
            mongoDatabase.GetCollection<MultipleChoiceQuestionDbModel>(databaseSettings.Value
                .MultipleChoiceQuestionsCollectionName);
    }

    public async Task<OrderQuestionDbModel> GetRandomOrderQuestionExcept(IEnumerable<string> excludedIds)
    {
        return await _orderQuestionsCollection.AsQueryable()
            .Where(q => !excludedIds.Contains(q.Id!))
            .Sample(1)
            .SingleAsync();
    }

    public async Task<MultipleChoiceQuestionDbModel> GetMultipleChoiceQuestionExcept(QuestionDifficulty difficulty,
        IEnumerable<string> excludedIds)
    {
        return await _multipleChoiceQuestionsCollection.AsQueryable()
            .Where(q => q.Difficulty == (int)difficulty)
            .Where(q => !excludedIds.Contains(q.Id!))
            .Sample(1)
            .SingleAsync();
    }
}