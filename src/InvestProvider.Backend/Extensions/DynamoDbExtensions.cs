using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Extensions;

public static class DynamoDbExtensions
{
    public static async Task<List<T>> BatchLoadAsync<T>(this IDynamoDBContext dynamoDb, IEnumerable<object> keys)
        where T : class
    {
        var tasks = keys.Select(k => dynamoDb.LoadAsync<T>(k));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList()!;
    }
}