using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Moq;
using Xunit;
using InvestProvider.Backend.Extensions;
using InvestProvider.Backend.Services.DynamoDb.Models;

namespace InvestProvider.Backend.Tests.Extensions;

public class DynamoDbExtensionsTests
{
    [Fact]
    public async Task BatchLoadAsync_LoadsEachKeyAndReturnsResults()
    {
        var dynamoDb = new Mock<IDynamoDBContext>();
        var keys = new[] { "k1", "k2" };
        var item1 = new ProjectsInformation { ProjectId = "k1", PoolzBackId = 1 };
        var item2 = new ProjectsInformation { ProjectId = "k2", PoolzBackId = 2 };

        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("k1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(item1);
        dynamoDb.Setup(x => x.LoadAsync<ProjectsInformation>("k2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(item2);

        var result = await dynamoDb.Object.BatchLoadAsync<ProjectsInformation>(keys);

        Assert.Equal(new[] { item1, item2 }, result);
        dynamoDb.Verify(x => x.LoadAsync<ProjectsInformation>("k1", It.IsAny<CancellationToken>()), Times.Once);
        dynamoDb.Verify(x => x.LoadAsync<ProjectsInformation>("k2", It.IsAny<CancellationToken>()), Times.Once);
    }
}
