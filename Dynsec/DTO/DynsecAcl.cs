using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Acl
{
    [JsonPropertyName("acltype")]
    public string Type { get; set; }

    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("priority")]
    public byte? Priority { get; set; }

    [JsonPropertyName("allow")]
    public bool Allow { get; set; }
}