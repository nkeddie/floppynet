using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FloppyNet.Aws.ReminderLambda;

public class Function
{
    private static string? DomainRoot = null;
    private static string? BotCredentials = null;
    private static string? ChatId = null;
    private static HttpClient client = new HttpClient();

    public static async Task Main()
    {
        DomainRoot = Environment.GetEnvironmentVariable("DomainRoot")
            ?? throw new Exception("DomainRoot was not supplied");

        BotCredentials = Environment.GetEnvironmentVariable("Telegram__BotCredentials")
            ?? throw new Exception("BotCredentials were not supplied");
        
        ChatId = Environment.GetEnvironmentVariable("Telegram__ChatId")
            ?? throw new Exception("ChatId was not supplied");

        Func<ILambdaContext, Task<string>> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static async Task<string> FunctionHandler(ILambdaContext context)
    {
        var uri = $"https://api.telegram.org/{BotCredentials}/sendPhoto?chat_id={ChatId}";
        var content = new MultipartFormDataContent();

        FontCollection collection = new();
        FontFamily family = collection.Add("bahnschrift.ttf");
        Font font = family.CreateFont(40, FontStyle.Bold);

        var image = Image.Load("input.png");

        var stats = await client.GetFromJsonAsync($"https://{DomainRoot}/leaderboard.json", LambdaFunctionJsonSerializerContext.Default.WordleStatArray)
            ?? throw new Exception("Failed to request leaderboard data");

        stats = stats.OrderBy(s => s.Average).ToArray();

        var users = await client.GetFromJsonAsync($"https://{DomainRoot}/users.json", LambdaFunctionJsonSerializerContext.Default.UserArray)
            ?? throw new Exception("Failed to request user data");

        int yOffset = 275;
        int yDelta = 100;

        // Headers
        image.Mutate(x => x.DrawText("User", font, Color.DarkGray, new PointF(300, 175)));
        image.Mutate(x => x.DrawText("Average", font, Color.DarkGray, new PointF(750, 175)));

        for (var i = 0; i < stats.Length; i++)
        {
            var stat = stats[i];
            var user = users.First(u => u.UserId == stat.UserId);
            image.Mutate(x => x.DrawText($"{i+1}. {user.DisplayName}", font, Color.Black, new PointF(300, yOffset)));
            image.Mutate(x => x.DrawText($"{stat.Average}", font, Color.Black, new PointF(750, yOffset)));
            yOffset += yDelta;
        }

        using var memoryStream = new MemoryStream();
        
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var streamContent = new StreamContent(memoryStream);
        content.Add(streamContent, "photo", "image.png");
        var response = await client.PostAsync(uri, content);

        var data = await response.Content.ReadAsStringAsync();

        return data;
    }
}

public record WordleStat(int UserId, decimal Average, decimal Min, decimal Max);
public record User(int UserId, string DisplayName);

[JsonSerializable(typeof(WordleStat[]))]
[JsonSerializable(typeof(User[]))]
[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}