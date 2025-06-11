using MediatR;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;
using InvestProvider.Backend.Services.Handlers.MyUpcomingAllocation.Models;

namespace InvestProvider.Backend.Services.Handlers.MyUpcomingAllocation;

public class MyUpcomingAllocationHandler(IDynamoDBContext dynamoDb)
    : IRequestHandler<MyUpcomingAllocationRequest, ICollection<MyUpcomingAllocationResponse>>
{
    public async Task<ICollection<MyUpcomingAllocationResponse>> Handle(MyUpcomingAllocationRequest request, CancellationToken cancellationToken)
    {
        var table = dynamoDb.GetTargetTable<WhiteList>();
        var scanDocument = await table.Query(new QueryOperationConfig 
        {
            IndexName = "UserAddress-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "UserAddress = :ua",
                ExpressionAttributeValues  = new Dictionary<string, DynamoDBEntry> { [":ua"] = request.UserAddress.Address }
            },
            FilterExpression = new Expression
            {
                ExpressionStatement = string.Join(" OR ", request.ProjectIDs.Select((_, i) => $"begins_with(HashId,:p{i})")),
                ExpressionAttributeValues = request.ProjectIDs
                    .Select((id, i) => new { k = $":p{i}", v = (DynamoDBEntry)id })
                    .ToDictionary(x => x.k, x => x.v)
            }
        }).GetRemainingAsync(cancellationToken);

        var whiteList = scanDocument.Select(dynamoDb.FromDocument<WhiteList>);

        var amountByProject = whiteList
            .GroupBy(w => w.HashId[..w.HashId.IndexOf('-')])
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var poolzBackIDs = await dynamoDb.BatchLoadAsync<ProjectsInformation>(
            request.ProjectIDs,
            cancellationToken
        );

        return poolzBackIDs.Select(p => new MyUpcomingAllocationResponse(
            p.ProjectId,
            p.PoolzBackId,
            amountByProject.GetValueOrDefault(p.ProjectId, 0m))
        ).ToArray();
    }
}