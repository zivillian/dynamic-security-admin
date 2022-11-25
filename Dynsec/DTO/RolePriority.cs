using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class RolePriority
{
    [JsonPropertyName("rolename")]
    public string Rolename { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; } = -1;
}