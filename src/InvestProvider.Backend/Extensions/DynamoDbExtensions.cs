using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Extensions;

public static class DynamoDbExtensions
{
    public static async Task<List<T>> BatchLoadAsync<T>(this IDynamoDBContext dynamoDb, IEnumerable<object> keys, CancellationToken ct = default)
    {
        var batch = dynamoDb.CreateBatchGet<T>();
        foreach (var k in keys) batch.AddKey(k);
        await batch.ExecuteAsync(ct);
        return batch.Results;
    }
}