using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Acl
{
    [JsonPropertyName("acltype")]
    public AclType Type { get; set; }

    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; } = -1;

    [JsonPropertyName("allow")]
    public bool Allow { get; set; }
}