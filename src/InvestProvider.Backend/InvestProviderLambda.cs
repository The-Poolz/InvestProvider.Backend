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

    public async Task<LambdaResponse> RunAsync(LambdaRequest request)
    {
        try
        {
            var response = await serviceProvider
                .GetRequiredService<IMediator>()
                .Send(request.HandlerRequest);
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