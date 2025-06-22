using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using InvestProvider.Backend.Services.Web3.Contracts;
using NethereumGenerators.Interfaces;
using InvestProvider.Backend.Services.Web3.Contracts.Models;
using InvestProvider.Backend.Services.Web3.Eip712;
using InvestProvider.Backend.Services.Web3.Eip712.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using InvestProvider.Backend.Services.DynamoDb.Models;
using Net.Web3.EthereumWallet;
using InvestProvider.Backend.Tests;

namespace InvestProvider.Backend.Tests.Handlers;

public class GenerateSignatureHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsResponse_WithExpectedFields()
    {
        var phase = TestHelpers.CreatePhase("1", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 0m);
        var projectInfo = TestHelpers.CreateProjectInfo(1, phase);

        var chainProvider = new Mock<IChainProvider<ContractType>>();
        chainProvider.Setup(x => x.ContractAddress(1, ContractType.InvestedProvider))
                     .Returns(new EthereumAddress("0x00000000000000000000000000000000000000aa"));

        var signatureGenerator = new Mock<ISignatureGenerator>();
        signatureGenerator.Setup(x => x.GenerateSignature(It.IsAny<Eip712Domain>(), It.IsAny<InvestMessage>()))
                          .Returns("sig");

        var handler = new GenerateSignatureHandler(chainProvider.Object, signatureGenerator.Object);
        var request = new GenerateSignatureRequest("pid", new EthereumAddress("0x0000000000000000000000000000000000000123"), "0");
        request.StrapiProjectInfo = projectInfo;
        request.DynamoDbProjectsInfo = new ProjectsInformation { ProjectId = "pid", PoolzBackId = 5 };
        request.Amount = 10;
        request.TokenDecimals = 18;
        request.UserInvestments = Array.Empty<UserInvestments>();

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal("sig", result.Signature);
        Assert.Equal(new DateTimeOffset(((dynamic)phase).Finish).ToUnixTimeSeconds(), result.ValidUntil);
        Assert.Equal(5, result.PoolzBackId);
        signatureGenerator.Verify(x => x.GenerateSignature(It.IsAny<Eip712Domain>(), It.IsAny<InvestMessage>()), Times.Once);
    }
}
