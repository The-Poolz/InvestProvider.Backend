using Net.Utils.ErrorHandler.Attributes;

namespace InvestProvider.Backend;

public enum Error
{
    [Error("ChainId not supported.")]
    CHAIN_NOT_SUPPORTED,
    [Error("No one implemented request found.")]
    INVALID_HANDLER_REQUEST,
    [Error("InvestedProvider contract in the selected chain not supported.")]
    INVESTED_PROVIDER_NOT_SUPPORTED,
    [Error("LockDealNFT contract in the selected chain not supported.")]
    LOCK_DEAL_NFT_NOT_SUPPORTED,
    [Error("Invest amount must be greater.")]
    INVEST_AMOUNT_IS_LESS_THAN_ALLOWED,
    [Error("Project phase not found.")]
    PROJECT_PHASE_NOT_FOUND,
    [Error("Phase inactive.")]
    PHASE_INACTIVE,
    [Error("User for selected phase not found.")]
    USER_NOT_FOUND
}