using Newtonsoft.Json;

namespace InvestProvider.Backend.Services.Handlers.AdminWriteAllocation.Models;

public class UserWithAmount
{
    [JsonRequired]
    public string UserAddress { get; set; } = null!;

    [JsonRequired]
    public decimal Amount { get; set; }
}