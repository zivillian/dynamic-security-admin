using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class GroupPriority
{
    [JsonPropertyName("groupname")]
    public string Groupname { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; } = -1;
}