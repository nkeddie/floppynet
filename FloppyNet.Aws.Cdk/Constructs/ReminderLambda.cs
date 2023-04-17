using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;

namespace FloppyNet.Aws.Cdk.Constructs
{
    public record ReminderLambdaProps(string Domain, string BotCredentials, string ChatId);

    public class ReminderLambda : Construct
    {
        public ReminderLambda(Construct scope, string id, ReminderLambdaProps props) : base(scope, id)
        {
            ArgumentNullException.ThrowIfNull(props.Domain);
            ArgumentNullException.ThrowIfNull(props.BotCredentials);
            ArgumentNullException.ThrowIfNull(props.ChatId);

            var reminderLambda = new Function(this, "Lambda", new FunctionProps
            {
                Runtime = Runtime.PROVIDED_AL2,
                Code = Code.FromAsset("output\\FloppyNet.Aws.ReminderLambda.zip"),
                Handler = "bootstrap",
                Timeout = Duration.Seconds(300),
                Environment = new Dictionary<string, string> {
                { "DomainRoot", props.Domain },
                { "Telegram__BotCredentials", props.BotCredentials },
                { "Telegram__ChatId", props.ChatId }
            },
                MemorySize = 512
            });

            var reminderRule = new Rule(this, "Rule", new RuleProps
            {
                Schedule = Schedule.Cron(new CronOptions
                {
                    Minute = "0",
                    Hour = "20"
                }),
            });

            reminderRule.AddTarget(new LambdaFunction(reminderLambda));
        }
    }
}
