using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using FloppyNet.Aws.WordleLambda.Models.Telegram;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FloppyNet.Aws.WordleLambda;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and returns both the upper and lower case version of the string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        context.Logger.Log($"Worldle function read {sqsEvent.Records.Count} record(s)");

        var s3Client = new AmazonS3Client();
        var handler = new WordleHandler(context, s3Client, "natekeddie.com");
        var response = new SQSBatchResponse();

        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var request = JsonSerializer.Deserialize<TelegramRequest>(record.Body);

                var message = request?.Message?.Text;
                if (message == null)
                {
                    context.Logger.Log($"{record.MessageId}: Message is not in the expected telegram API format. Message text is empty");
                    continue;
                }

                var userId = request?.Message?.From?.Id;
                if (userId == null)
                {
                    context.Logger.Log($"{record.MessageId}: Message is not in the expected telegram API format. From.Id is empty");
                    continue;
                }
                await handler.ExecuteAsync(userId.Value, message);
            }
            catch (Exception e)
            {
                context.Logger.LogError($"{e.Message}: {e.StackTrace}");
                response.BatchItemFailures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = record.MessageId
                });
            }
        }
        context.Logger.Log($"Worldle function exited with {response.BatchItemFailures.Count} failure(s)");
        return response;
    }
}
