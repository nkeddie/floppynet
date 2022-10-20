using System.Text.Json.Serialization;

namespace FloppyNet.Aws.WordleLambda.Models.Telegram;

public record From
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}