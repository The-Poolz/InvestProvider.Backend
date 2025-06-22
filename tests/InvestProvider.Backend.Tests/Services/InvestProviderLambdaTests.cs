using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;
using InvestProvider.Backend.Models;
using InvestProvider.Backend;
using Microsoft.Extensions.DependencyInjection;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;

namespace InvestProvider.Backend.Tests.Services;

public class InvestProviderLambdaTests
{
    [Fact]
    public async Task RunAsync_ReturnsHandlerResponse()
    {
        var services = new ServiceCollection();
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync("ok");
        services.AddSingleton(mediator.Object);
        var lambda = new InvestProviderLambda(services.BuildServiceProvider());

        var req = new LambdaRequest { MyAllocation = new InvestProvider.Backend.Services.Handlers.MyAllocation.Models.MyAllocationRequest("p", new Net.Web3.EthereumWallet.EthereumAddress("0x0000000000000000000000000000000000000001")) };
        var response = await lambda.RunAsync(req);

        Assert.Equal("ok", response.HandlerResponse);
    }

    [Fact]
    public async Task RunAsync_ReturnsError_OnValidationException()
    {
        var services = new ServiceCollection();
        var mediator = new Mock<IMediator>();
        var failures = new[] { new FluentValidation.Results.ValidationFailure("field", "err") { ErrorCode = "code" } };
        mediator.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ValidationException(failures));
        services.AddSingleton(mediator.Object);
        var lambda = new InvestProviderLambda(services.BuildServiceProvider());

        var req = new LambdaRequest { MyAllocation = new InvestProvider.Backend.Services.Handlers.MyAllocation.Models.MyAllocationRequest("p", new Net.Web3.EthereumWallet.EthereumAddress("0x0000000000000000000000000000000000000001")) };
        var response = await lambda.RunAsync(req);

        Assert.Equal("err", response.ErrorMessage);
        Assert.Equal("code", response.ErrorType);
    }
}
