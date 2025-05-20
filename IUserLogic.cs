using System;
using System.Linq;

namespace InvestProvider.Backend
{
    public interface IUserLogic
    {
        public const decimal minInvest = 1;
        public IRPCAPI RpcApi { get; }
        public DataBase Db { get; }
        public (decimal,DateTime,DateTime)[] GetAllocation(string Address, int ProjectID)
        {
            return Db.PhaseList.Where(x => x.ProjectId == ProjectID)
                .Select(x => (x.IsWhiteList ? Db.UserDataList.FirstOrDefault(u => u.PhaseId == x.Id && u.Address == Address).Amount 
                : x.MaxInvest,
                x.Start, x.End)).ToArray();
        }
        public string GetSignature(string Address, int ProjectId, decimal Value)
        {
            if (Value < minInvest)
            {
                throw new ArgumentOutOfRangeException("Value", "Value must be greater than 1");
            }
            var currentPhase = Db.PhaseList.Where(x => x.ProjectId == ProjectId && x.Start <= DateTime.Now && x.End > DateTime.Now).FirstOrDefault() 
                ?? throw new Exception("No Active Phase");
            var poolId = Db.Projects.FirstOrDefault(x => x.Id == ProjectId).PoolId;
            var userInvests = RpcApi.GetUserInvests(poolId, Address);
            var sumInvest = userInvests.Where(x => x.Item1 >= currentPhase.Start && x.Item1 < currentPhase.End).Sum(x => x.Item2);
            var userData = currentPhase.UserDatas.FirstOrDefault(x => x.Address == Address);
            if (currentPhase.IsWhiteList)
            {
                ValidateWhiteList( userData, Value, sumInvest);
            }
            else
            {
                ValidateFCFS(currentPhase, Value, sumInvest);
            }
            return $"{poolId} {Address} {currentPhase.End} {Value} {userInvests.Count()}"; // sign this
            // https://github.com/The-Poolz/LockDealNFT.InvestProvider/blob/46fa5d695749ce32ff135a133ffbf26f6e4a5507/contracts/InvestModifiers.sol#L112
        }

        private static void ValidateFCFS(Phase currentPhase, decimal Value, decimal sumInvest)
        {
            if (currentPhase.MaxInvest > Value)
            {
                throw new ArgumentOutOfRangeException($"Value exceed the MaxInvest = {currentPhase.MaxInvest}");
            }
            if (sumInvest > 0)
            {
                throw new ArgumentOutOfRangeException("sumInvest", "Already Invested");
            }
        }

        private void ValidateWhiteList(UserData userData, decimal Value, decimal sumInvest)
        {            
            if (userData == null)
            {
                throw new ArgumentOutOfRangeException("User not in WhiteList");
            }
            if (userData.Amount < Value + sumInvest)
            {
                throw new ArgumentOutOfRangeException("Value exceed the Amount {Value} {sumInvest} {userData.Amount}");
            }
        }
    }
}
