﻿using Net.Utils.ErrorHandler.Extensions;
using InvestProvider.Backend.Services.Handlers.GenerateSignature.Models;
using InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

namespace InvestProvider.Backend.Models;

public class LambdaRequest
{
    public GenerateSignatureRequest? GenerateSignature { get; set; }
    public AdminWriteAllocationRequest? AdminWriteAllocation { get; set; }

    public object HandlerRequest => this switch
    {
        { GenerateSignature: not null } => GenerateSignature,
        { AdminWriteAllocation: not null } => AdminWriteAllocation,
        _ => throw Error.INVALID_HANDLER_REQUEST.ToException()
    };
}