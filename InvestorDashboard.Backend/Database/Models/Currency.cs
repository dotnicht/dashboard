using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace InvestorDashboard.Backend.Database.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Currency
    {
        [EnumMember(Value = "USD")]
        USD = 0,
        [EnumMember(Value = "ETH")]
        ETH = 1,
        [EnumMember(Value = "BTC")]
        BTC = 2,
        Token = int.MaxValue
    }
}
