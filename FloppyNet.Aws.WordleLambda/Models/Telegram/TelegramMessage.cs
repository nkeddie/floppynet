using System.Text.Json.Serialization;

namespace FloppyNet.Aws.WordleLambda.Models.Telegram;

public record TelegramMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("date")]
    public decimal Date { get; set; }

    [JsonPropertyName("from")]
    public From? From { get; set; }
}
