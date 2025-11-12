using Net.Web3.EthereumWallet;

namespace InvestProvider.Backend.Services.Strapi.Models;

public record OnChainInfo(EthereumAddress InvestedProvider, EthereumAddress LockDealNFT);