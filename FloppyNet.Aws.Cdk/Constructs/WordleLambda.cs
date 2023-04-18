using Amazon.CDK;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SQS;

namespace FloppyNet.Aws.Cdk.Constructs
{
    public record WordleLambdaProps(Queue Queue);

    public class WordleLambda : Construct
    {
        public Function Lambda { get; }

        public WordleLambda(Construct scope, string id, WordleLambdaProps props) : base(scope, id)
        {
            Lambda = new Function(this, "Lambda", new FunctionProps
            {
                Runtime = Runtime.DOTNET_6,
                Code = Code.FromAsset(Path.Combine("output", "FloppyNet.Aws.WordleLambda")),
                Handler = "FloppyNet.Aws.WordleLambda::FloppyNet.Aws.WordleLambda.Function::FunctionHandler",
                Timeout = Duration.Seconds(300),
                MemorySize = 512
            });


            Lambda.AddEventSource(new SqsEventSource(props.Queue, new SqsEventSourceProps
            {
                ReportBatchItemFailures = true
            }));
        }
    }
}
