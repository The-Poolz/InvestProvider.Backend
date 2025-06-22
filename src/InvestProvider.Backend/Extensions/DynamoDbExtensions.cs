using Amazon.DynamoDBv2.DataModel;
using System.Linq;

namespace InvestProvider.Backend.Extensions;

public static class DynamoDbExtensions
{
    public static async Task<List<T>> BatchLoadAsync<T>(this IDynamoDBContext dynamoDb, IEnumerable<object> keys, CancellationToken ct = default)
        where T : class
    {
        var tasks = keys.Select(k => dynamoDb.LoadAsync<T>(k, ct));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList()!;
    }
}