using MediatR;
using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Strapi;
using InvestProvider.Backend.Services.Web3.Contracts;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Services.Handlers.GenerateSignature;

public class GenerateSignatureHandler(IStrapiClient strapi, IInvestProviderContract investProvider)
    : IRequestHandler<GenerateSignatureRequest, GenerateSignatureResponse>
{
    public const byte MinInvestAmount = 1;

    public Task<GenerateSignatureResponse> Handle(GenerateSignatureRequest request, CancellationToken cancellationToken)
    {
        var phase = strapi.ReceiveProjectPhase(request.PhaseId);
        if (phase.StartTime >= DateTime.UtcNow || phase.EndTime < DateTime.UtcNow)
        {
            throw Error.PHASE_INACTIVE.ToException(new
            {
                request.PhaseId,
                phase.StartTime,
                phase.EndTime
            });
        }

        var amount = 123m; // TODO: Receive from cache token decimals and parse WeiAmount
        if (amount < MinInvestAmount)
        {
            throw Error.INVEST_AMOUNT_IS_LESS_THAN_ALLOWED.ToException(new
            {
                UserAmount = amount,
                MinInvestAmount
            });
        }

        var userInvestments = investProvider.GetUserInvests(request.)
    }
}