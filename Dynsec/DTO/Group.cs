using System.Text.Json.Serialization;

namespace Dynsec.DTO;

public class Group
{
    [JsonPropertyName("groupname")]
    public string Groupname { get; set; }
}