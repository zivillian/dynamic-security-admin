using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Role
{
    [JsonPropertyName("rolename")]
    public string Rolename { get; set; }

    [JsonPropertyName("textname")]
    public string? Name { get; set; }

    [JsonPropertyName("textdescription")]
    public string? Description { get; set; }

    [JsonPropertyName("acls")]
    public Acl[]? Acls { get; set; }
}