using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FloppyNet.Aws.TelegramLambda;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and returns both the upper and lower case version of the string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Exit if request is not of type POST
        if (!"POST".Equals(request.RequestContext.HttpMethod, StringComparison.OrdinalIgnoreCase))
        {
            return new APIGatewayProxyResponse { StatusCode = (int)HttpStatusCode.NotFound, Body = "Not found." };
        }

        request.QueryStringParameters.TryGetValue("token", out string? token);

        string secretKey = Environment.GetEnvironmentVariable("TELEGRAM_SECRET_KEY") ?? throw new Exception("TELEGRAM_SECRET_KEY is not defined");
        string queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL") ?? throw new Exception("QUEUE_URL is not defined");
        var client = new AmazonSQSClient();

        if (token != secretKey) throw new Exception("Telegram secret key is incorrect");

        // Push message to queue
        await SendMessage(client, queueUrl, request.Body);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = $"Ack."
        };
    }


    static async Task SendMessage(AmazonSQSClient sqsClient, string queueUrl, string messageBody)
    {
        await sqsClient.SendMessageAsync(queueUrl, messageBody);
    }
}