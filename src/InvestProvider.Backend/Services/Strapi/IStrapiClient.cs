using InvestProvider.Backend.Services.Strapi.Models;

namespace InvestProvider.Backend.Services.Strapi;

public interface IStrapiClient
{
    Task<OnChainInfo> ReceiveOnChainInfoAsync(long chainId);
    Task<ProjectInfo> ReceiveProjectInfoAsync(string projectId, bool filterPhases);
}