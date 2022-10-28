using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MultiMillionaire.Database;

namespace MultiMillionaire.Services;

public interface IDatabaseService
{
    public Task<OrderQuestionDbModel> GetRandomOrderQuestionExcept(IEnumerable<string> excludedIds);
}

public class DatabaseService : IDatabaseService
{
    private readonly IMongoCollection<OrderQuestionDbModel> _questionsCollection;

    public DatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _questionsCollection =
            mongoDatabase.GetCollection<OrderQuestionDbModel>(databaseSettings.Value.OrderQuestionsCollectionName);
    }

    public async Task<OrderQuestionDbModel> GetRandomOrderQuestionExcept(IEnumerable<string> excludedIds)
    {
        return await _questionsCollection.AsQueryable().Where(q => !excludedIds.Contains(q.Id!)).Sample(1)
            .SingleAsync();
    }
}