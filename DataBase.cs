using System;
using System.Collections.Generic;

namespace InvestProvider.Backend
{
    public class DataBase
    {
        public List<Project> Projects;
        public List<Phase> PhaseList;
        public List<UserData> UserDataList;
    }
    public class Project
    {
        public int Id { get; set; } // this must sync with Strapi
        public int PoolId { get; set; }
        public virtual List<Phase> Phases { get; set; }
    }

    public class Phase
    {
        public string Id => $"{ProjectId}{Start}{End}"; // Hash of ProjectId, Start and End
        public int ProjectId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool IsWhiteList => MaxInvest == 0;
        public decimal MaxInvest { get; set; } // null when IsWhiteList = true
        public virtual List<UserData> UserDatas { get; set; }
    }
    public class UserData
    {
        public string Id =>  $"{PhaseId}{Address}"; // Hash of Address and PhaseId
        public string PhaseId { get; set; }
        public string Address { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
