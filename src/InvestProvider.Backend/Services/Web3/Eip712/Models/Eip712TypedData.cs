using Newtonsoft.Json;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json.Linq;

namespace InvestProvider.Backend.Services.Web3.Eip712.Models;

[JsonObject(MemberSerialization.OptIn)]
public class Eip712TypedData : TypedData<Eip712Domain>
{
    public Eip712TypedData(Eip712Domain domain, InvestMessage investMessage)
    {
        Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(InvestMessage), typeof(Eip712Domain));
        Domain = domain;
        DomainRawValues = MemberValueFactory.CreateFromMessage(Domain);
        PrimaryType = nameof(InvestMessage);
        Message = MemberValueFactory.CreateFromMessage(investMessage);
    }

    public string ToEip712Json()
    {
        var originalObj = JObject.Parse(this.ToJson());
        var reorderedObj = new JObject
        {
            ["types"] = originalObj["types"],
            ["domain"] = originalObj["domain"],
            ["primaryType"] = originalObj["primaryType"],
            ["message"] = originalObj["message"]
        };
        return reorderedObj.ToString(Formatting.Indented);
    }
}