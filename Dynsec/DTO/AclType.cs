using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynsec.DTO;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum AclType
{
    [EnumMember(Value = "publishClientSend")]
    PublishClientSend,
    [EnumMember(Value = "publishClientReceive")]
    PublishClientReceive,
    [EnumMember(Value = "subscribeLiteral")]
    SubscribeLiteral,
    [EnumMember(Value = "subscribePattern")]
    SubscribePattern,
    [EnumMember(Value = "unsubscribeLiteral")]
    UnsubscribeLiteral,
    [EnumMember(Value = "unsubscribePattern")]
    UnsubscribePattern,
    [EnumMember(Value = "subscribe")]
    Subscribe,
    [EnumMember(Value = "unsubscribe")]
    Unsubscribe,
}