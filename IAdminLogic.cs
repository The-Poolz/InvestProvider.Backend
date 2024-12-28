using System;
using System.Collections.Generic;

namespace InvestProvider.Backend
{
    public interface IAdminLogic
    {
        public void AddProject(int ProjectId, int PoolId);
        public void DeleteProject(int ProjectId);
        public void AddPhase(int ProjectId,DateTime start, DateTime end, bool isWhiteList);
        public void DeletePhase(int id);
        public void AddWhitelist(int PhaseId, (string address, string weiAmount)[] values); // need to be IsWhiteList = true
        public List<Phase> GetPhases(int[] ProjectIds);       
    }
}
