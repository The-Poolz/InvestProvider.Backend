using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;

namespace InvestProvider.Backend.Models;

public class LambdaRequest
{
    public GenerateSignatureRequest? GenerateSignature { get; set; }

    public object HandlerRequest => this switch
    {
        { GenerateSignature: not null } => GenerateSignature,
        _ => throw Error.INVALID_HANDLER_REQUEST.ToException()
    };
}