using MediatR;
using Amazon.DynamoDBv2.DataModel;
using InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId.Models;

namespace InvestProvider.Backend.Services.Handlers.AdminCreatePoolzBackId;

public class AdminCreatePoolzBackIdHandler(IDynamoDBContext dynamoDb)
    : IRequestHandler<AdminCreatePoolzBackIdRequest, AdminCreatePoolzBackIdResponse>
{
    public async Task<AdminCreatePoolzBackIdResponse> Handle(AdminCreatePoolzBackIdRequest request, CancellationToken cancellationToken)
    {
        await dynamoDb.SaveAsync(request, cancellationToken);

        return new AdminCreatePoolzBackIdResponse(request);
    }
}