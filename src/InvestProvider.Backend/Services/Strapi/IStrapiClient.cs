using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Strapi;

public interface IStrapiClient
{
    public OnChainInfo ReceiveOnChainInfo(long chainId);
    public ProjectPhase ReceiveProjectPhase(string phaseId);
}