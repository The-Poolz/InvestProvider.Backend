using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace InvestProvider.Backend.Tests;

public class StubAsyncSearch<T> : AsyncSearch<T>
{
    private readonly List<T> _results;

    public StubAsyncSearch(IEnumerable<T> results)
    {
        _results = new List<T>(results);
    }

    public override Task<List<T>> GetNextSetAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_results);
    }

    public override Task<List<T>> GetRemainingAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_results);
    }
}
