using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;

namespace FloppyNet.Aws.Cdk.Constructs
{
    public record ApiProps(IHostedZone HostedZone, string Domain, Certificate Certificate, Function TelegramLambda);
    public class Api : Construct
    {
        public Api(Construct scope, string id, ApiProps props) : base(scope, id)
        {
            ArgumentNullException.ThrowIfNull(props.Domain);
            ArgumentNullException.ThrowIfNull(props.Certificate);
            ArgumentNullException.ThrowIfNull(props.HostedZone);
            ArgumentNullException.ThrowIfNull(props.TelegramLambda);

            var api = new RestApi(this, "Api", new RestApiProps
            {
                RestApiName = "Floppy Net",
                Description = "An api layer over floppy net."
            });

            api.AddDomainName("ApiGatewayDomainName", new DomainNameOptions
            {
                DomainName = $"api.{props.Domain}",
                Certificate = props.Certificate
            });

            new ARecord(this, "ApiRecord", new ARecordProps
            {
                Zone = props.HostedZone,
                RecordName = $"api.{props.Domain}",
                Target = RecordTarget.FromAlias(new ApiGateway(api))
            });


            var telegramPath = api.Root.AddResource("telegram");

            var telegramLambdaIntegration = new LambdaIntegration(props.TelegramLambda, new LambdaIntegrationOptions
            {
                RequestTemplates = new Dictionary<string, string> { ["application/json"] = "{ \"statusCode\": \"200\" }" }
            });

            telegramPath.AddProxy(new ProxyResourceOptions
            {
                DefaultIntegration = telegramLambdaIntegration
            });

        }
    }
}
