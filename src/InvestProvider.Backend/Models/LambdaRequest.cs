using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;

namespace InvestProvider.Backend.Models;

public class LambdaRequest
{
    public GenerateSignatureRequest? GenerateSignature { get; set; }
    public AdminGetAllocationRequest? AdminGetAllocation { get; set; }

    public object HandlerRequest => this switch
    {
        { GenerateSignature: not null } => GenerateSignature,
        { AdminGetAllocation: not null } => AdminGetAllocation,
        _ => throw Error.INVALID_HANDLER_REQUEST.ToException()
    };
}