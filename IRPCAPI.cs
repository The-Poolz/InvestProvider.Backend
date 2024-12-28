using System;

namespace InvestProvider.Backend
{
    public interface IRPCAPI
    {       
        public (DateTime, decimal)[] GetUserInvests(int poolId, string Address); 
        // https://github.com/The-Poolz/LockDealNFT.InvestProvider/blob/46fa5d695749ce32ff135a133ffbf26f6e4a5507/contracts/InvestNonce.sol#L59C14-L59C28
    }
}
