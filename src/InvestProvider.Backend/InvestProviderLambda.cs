using MediatR;
using FluentValidation;
using Amazon.Lambda.Core;
using InvestProvider.Backend.Models;
using InvestProvider.Backend.Services;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace InvestProvider.Backend;

public class InvestProviderLambda(IServiceProvider serviceProvider)
{
    public InvestProviderLambda() : this(DefaultServiceProvider.Build()) { }
    internal IMediator Mediator => serviceProvider.GetRequiredService<IMediator>();
    public async Task<LambdaResponse> RunAsync(LambdaRequest request)
    {
        try
        {
            var response = await Mediator.Send(request.HandlerRequest);
            return new LambdaResponse(response);
        }
        catch (ValidationException ex)
        {
            return new LambdaResponse(ex);
        }
        catch (Exception ex)
        {
            LambdaLogger.Log(ex.ToString());
            return new LambdaResponse(ex);
        }
    }
}