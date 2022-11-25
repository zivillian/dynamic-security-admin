using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Client
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("clientid")]
    public string? ClientId { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("textname")]
    public string? Name { get; set; }

    [JsonPropertyName("textdescription")]
    public string? Description { get; set; }

    [JsonPropertyName("disabled")]
    public bool? Disabled { get; set; }

    [JsonPropertyName("roles")]
    public RolePriority[]? Roles { get; set; }

    [JsonPropertyName("groups")]
    public GroupPriority[]? Groups { get; set; }
}