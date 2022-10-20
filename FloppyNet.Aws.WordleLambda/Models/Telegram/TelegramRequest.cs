using System.Text.Json.Serialization;

namespace FloppyNet.Aws.WordleLambda.Models.Telegram;

public record TelegramRequest
{
    [JsonPropertyName("message")]
    public TelegramMessage? Message { get; set; }
}
