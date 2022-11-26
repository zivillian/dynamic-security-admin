using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Group
{
    [JsonPropertyName("groupname")]
    public string Groupname { get; set; }

    [JsonPropertyName("textname")]
    public string? Name { get; set; }

    [JsonPropertyName("textdescription")]
    public string? Description { get; set; }

    [JsonPropertyName("roles")]
    public RolePriority[]? Roles { get; set; }

    [JsonPropertyName("clients")]
    public ClientReference[]? Clients { get; set; }
}