using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MultiMillionaire.Database;

namespace MultiMillionaire.Services;

public interface IOrderQuestionsService
{
    public Task<OrderQuestionDbModel> GetRandom();
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

    public async Task<OrderQuestionDbModel> GetRandom()
    {
        return await _questionsCollection.AsQueryable().Sample(1).SingleAsync();
    }
}