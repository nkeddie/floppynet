using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;

namespace FloppyNet.Aws.Cdk.Constructs
{
    public record TelegramLambdaProps(string TelegramSecretKey);
    public class TelegramLambda : Construct
    {
        public Queue Queue { get; }
        public Function Lambda { get; }
        public TelegramLambda(Construct scope, string id, TelegramLambdaProps props) : base(scope, id)
        {
            ArgumentNullException.ThrowIfNull(props.TelegramSecretKey);

            Lambda = new Function(this, "Lambda", new FunctionProps
            {
                Runtime = Runtime.PROVIDED_AL2,
                Code = Code.FromAsset(Path.Combine("output", "FloppyNet.Aws.TelegramLambda.zip")),
                Handler = "bootstrap",
                Timeout = Duration.Seconds(300),
                MemorySize = 512
            });

            Queue = new Queue(this, "MessageQueue", new QueueProps
            {
                VisibilityTimeout = Duration.Seconds(500),
            });

            Queue.GrantSendMessages(Lambda);

            Lambda.AddEnvironment("QUEUE_URL", Queue.QueueUrl);
            Lambda.AddEnvironment("TELEGRAM_SECRET_KEY", props.TelegramSecretKey);
        }
    }
}
