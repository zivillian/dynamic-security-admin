using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class ClientReference
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
}