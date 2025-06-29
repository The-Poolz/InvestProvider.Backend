using MediatR;
using InvestProvider.Backend.Services.Handlers.MyAllocation.Models;
using InvestProvider.Backend.Services.Web3;
using InvestProvider.Backend.Services.Web3.Contracts;
using Net.Cache.DynamoDb.ERC20;
using poolz.finance.csharp.contracts.InvestProvider;
using poolz.finance.csharp.contracts.LockDealNFT;
using Net.Cache.DynamoDb.ERC20.Models;
using Nethereum.Util;
using InvestProvider.Backend.Services.Web3.Contracts.Models;

namespace InvestProvider.Backend.Services.Handlers.MyAllocation;

public class MyAllocationHandler(
    IRpcProvider rpcProvider,
    ERC20CacheProvider erc20Cache,
    ILockDealNFTService<ContractType> lockDealNFT,
    IInvestProviderService<ContractType> investProvider
)
    : IRequestHandler<MyAllocationRequest, MyAllocationResponse>
{
    public async Task<MyAllocationResponse> Handle(MyAllocationRequest request, CancellationToken cancellationToken)
    {
        var amount = 0m;
        if (request.WhiteList != null)
        {
            amount = request.WhiteList.Amount;
        }
        else
        {
            var tokenAddress = await lockDealNFT.TokenOfQueryAsync(
                request.StrapiProjectInfo.ChainId,
                ContractType.LockDealNFT,
                request.DynamoDbProjectsInfo.PoolzBackId
            );

            var tokenDecimals = erc20Cache.GetOrAdd(new GetCacheRequest(
                request.StrapiProjectInfo.ChainId,
                tokenAddress,
                rpcProvider.RpcUrl(request.StrapiProjectInfo.ChainId)
            )).Decimals;

            var userInvestResponse = await investProvider.GetUserInvestsQueryAsync(
                request.StrapiProjectInfo.ChainId,
                ContractType.InvestedProvider,
                request.DynamoDbProjectsInfo.PoolzBackId,
                request.UserAddress
            );

            var userInvestments = userInvestResponse.ReturnValue1
                .Select(ui => new UserInvestments(ui))
                .ToArray();

            var investedAmount = userInvestments
                .Where(ui => ui.BlockCreation >= request.StrapiProjectInfo.CurrentPhase!.Start && ui.BlockCreation < request.StrapiProjectInfo.CurrentPhase.Finish)
                .Sum(ui => UnitConversion.Convert.FromWei(ui.Amount, tokenDecimals));

            if (investedAmount == 0m)
            {
                amount = (decimal)request.StrapiProjectInfo.CurrentPhase!.MaxInvest!;
            }
        }

        var response = new MyAllocationResponse(
            amount: amount,
            startTime: request.StrapiProjectInfo.CurrentPhase!.Start!.Value,
            endTime: request.StrapiProjectInfo.CurrentPhase.Finish!.Value,
            poolzBackId: request.DynamoDbProjectsInfo.PoolzBackId
        );
        return response;
    }
}