using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Strapi;

public interface IStrapiClient
{
    public OnChainInfo ReceiveOnChainInfo(long chainId);
    public ProjectInfo ReceiveProjectInfo(string projectId, bool filterPhases);
}