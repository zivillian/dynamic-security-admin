using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Client
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("textname")]
    public string Name { get; set; }

    [JsonPropertyName("textdescription")]
    public string Description { get; set; }

    [JsonPropertyName("roles")]
    public Role[] Roles { get; set; }

    [JsonPropertyName("groups")]
    public Group[] Groups { get; set; }
}