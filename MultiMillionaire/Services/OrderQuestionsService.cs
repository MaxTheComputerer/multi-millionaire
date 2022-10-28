using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MultiMillionaire.Database;

namespace MultiMillionaire.Services;

public interface IOrderQuestionsService
{
    Task<List<OrderQuestionDbModel>> GetAsync();
}

public class OrderQuestionsService : IOrderQuestionsService
{
    private readonly IMongoCollection<OrderQuestionDbModel> _questionsCollection;

    public OrderQuestionsService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _questionsCollection =
            mongoDatabase.GetCollection<OrderQuestionDbModel>(databaseSettings.Value.OrderQuestionsCollectionName);
    }

    public async Task<List<OrderQuestionDbModel>> GetAsync()
    {
        return await _questionsCollection.Find(_ => true).ToListAsync();
    }
}