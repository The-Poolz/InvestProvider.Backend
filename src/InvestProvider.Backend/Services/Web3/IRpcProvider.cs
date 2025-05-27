namespace InvestProvider.Backend.Services.Web3;

public interface IRpcProvider
{
    public string RpcUrl(long chainId);
}