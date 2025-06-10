using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using InvestProvider.Backend.Services.Handlers.AdminGetAllocation.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Models;

public class LambdaRequest
{
    public GenerateSignatureRequest? GenerateSignature { get; set; }
    public AdminGetAllocationRequest? AdminGetAllocation { get; set; }
    public AdminWriteAllocationRequest? AdminWriteAllocation { get; set; }
    public MyAllocationRequest? MyAllocation { get; set; }
    public AdminCreatePoolzBackIdRequest? AdminCreatePoolzBackId { get; set; }

    public object HandlerRequest => this switch
    {
        { GenerateSignature: not null } => GenerateSignature,
        { AdminGetAllocation: not null } => AdminGetAllocation,
        { AdminWriteAllocation: not null } => AdminWriteAllocation,
        { MyAllocation: not null } => MyAllocation,
        { AdminCreatePoolzBackId: not null } => AdminCreatePoolzBackId,
        _ => throw Error.INVALID_HANDLER_REQUEST.ToException()
    };
}