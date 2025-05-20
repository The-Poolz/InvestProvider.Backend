using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Strapi.Models;

public record OnChainInfo(string RpcUrl, EthereumAddress InvestedProvider, EthereumAddress LockDealNFT);