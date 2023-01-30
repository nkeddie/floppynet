using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.SQS;
using Microsoft.Extensions.Configuration;
using Function = Amazon.CDK.AWS.Lambda.Function;
using FunctionProps = Amazon.CDK.AWS.Lambda.FunctionProps;
using LambdaFunctionTarget = Amazon.CDK.AWS.Events.Targets.LambdaFunction;

namespace FloppyNet.Aws.Cdk;

public class FloppyNetStack : Stack
{
    public FloppyNetStack(Construct scope, IConfiguration config, IStackProps? props = null) : base(scope, config["Stack:Name"], props)
    {
        string domain = config["Stack:DomainRoot"];
        string hostedZoneId = config["Stack:HostedZoneId"];

        var hostedZone = HostedZone.FromHostedZoneAttributes(this, "FloppyNetHostedZone", new HostedZoneAttributes
        {
            HostedZoneId = hostedZoneId,
            ZoneName = domain
        });

        var reminderLambda = new Function(this, "ReminderLambda", new FunctionProps
        {
            Runtime = Runtime.PROVIDED_AL2,
            Code = Code.FromAsset("output\\FloppyNet.Aws.ReminderLambda.zip"),
            Handler = "bootstrap",
            Timeout = Duration.Seconds(300),
            Environment = new Dictionary<string, string> {
                { "DomainRoot", domain },
                { "Telegram__BotCredentials", config["Telegram:BotCredentials"] },
                { "Telegram__ChatId", config["Telegram:ChatId"] }
            },
            MemorySize = 512
        });

        var reminderRule = new Rule(this, "ReminderRule", new RuleProps
        {
            Schedule = Schedule.Cron(new CronOptions
            {
                Minute = "0",
                Hour = "20"
            }),
        });

        reminderRule.AddTarget(new LambdaFunctionTarget(reminderLambda));

        var telegramLambda = new Function(this, "TelegramLambdaAot", new FunctionProps
        {
            Runtime = Runtime.PROVIDED_AL2,
            Code = Code.FromAsset("output\\FloppyNet.Aws.TelegramLambda.zip"),
            Handler = "bootstrap",
            Timeout = Duration.Seconds(300),
            MemorySize = 512
        });

        var wordleLambda = new Function(this, "WordleLambda", new FunctionProps
        {
            Runtime = Runtime.FROM_IMAGE,
            Code = Code.FromAssetImage("FloppyNet.Aws.WordleLambda\\"),
            Handler = Handler.FROM_IMAGE,
            Timeout = Duration.Seconds(300),
            MemorySize = 512
        });

        var queue = new Queue(this, "TelegramMessageQueue", new QueueProps
        {
            VisibilityTimeout = Duration.Seconds(500),
        });

        queue.GrantSendMessages(telegramLambda);

        telegramLambda.AddEnvironment("QUEUE_URL", queue.QueueUrl);
        telegramLambda.AddEnvironment("TELEGRAM_SECRET_KEY", config["Telegram:SecretKey"]);

        wordleLambda.AddEventSource(new SqsEventSource(queue, new SqsEventSourceProps
        {
            ReportBatchItemFailures = true
        }));

        var bucket = new Bucket(this, "WordleApp", new BucketProps
        {
            AccessControl = BucketAccessControl.PUBLIC_READ,
            BucketName = domain,
            WebsiteIndexDocument = "index.html",
            PublicReadAccess = true
        });

        bucket.GrantReadWrite(wordleLambda);
        bucket.GrantPutAcl(wordleLambda);

        var certificate = new Certificate(this, "FloppyNetApiCert", new CertificateProps
        {
            DomainName = $"*.{domain}",
            SubjectAlternativeNames = new[] { domain },
            Validation = CertificateValidation.FromDns(hostedZone)
        });

        var restApi = new RestApi(this, "FloppyNetApi", new RestApiProps
        {
            RestApiName = "Floppy Net",
            Description = "An api layer over floppy net."
        });

        restApi.AddDomainName("FloppyNetApiGatewayDomainName", new DomainNameOptions
        {
            DomainName = $"api.{domain}",
            Certificate = certificate
        });

        var cloudFrontOrigin = new S3Origin(bucket);

        var cloudFront = new Distribution(this, "FloppyNetWebDist", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
                Origin = cloudFrontOrigin,
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                CachePolicy = CachePolicy.CACHING_DISABLED
            },
            ErrorResponses = new[] {
                new ErrorResponse {
                    HttpStatus = 404,
                    Ttl = Duration.Seconds(0),
                    ResponseHttpStatus = 200,
                    ResponsePagePath = "/index.html"
                }
            },
            Certificate = certificate,
            DomainNames = new[] { domain }
        });


        new BucketDeployment(this, "StaticWebsite", new BucketDeploymentProps
        {
            DestinationBucket = bucket,
            Sources = new[] { Source.Asset("FloppyNet.UI\\floppy-net\\dist") },
            Prune = false,
            Distribution = cloudFront,
        });

        cloudFront.AddBehavior("assets/*", cloudFrontOrigin, new AddBehaviorOptions
        {
            CachePolicy = CachePolicy.CACHING_OPTIMIZED
        });

        cloudFront.AddBehavior("favicon.ico", cloudFrontOrigin, new AddBehaviorOptions
        {
            CachePolicy = CachePolicy.CACHING_OPTIMIZED
        });

        cloudFront.AddBehavior("index.html", cloudFrontOrigin, new AddBehaviorOptions
        {
            CachePolicy = CachePolicy.CACHING_OPTIMIZED
        });

        new ARecord(this, "FloppyNetApiRecord", new ARecordProps
        {
            Zone = hostedZone,
            RecordName = $"api.{domain}",
            Target = RecordTarget.FromAlias(new ApiGateway(restApi))
        });

        new ARecord(this, "FloppyNetWebRecord", new ARecordProps
        {
            Zone = hostedZone,
            RecordName = domain,
            Target = RecordTarget.FromAlias(new CloudFrontTarget(cloudFront))
        });

        var telegramLambdaIntegration = new LambdaIntegration(telegramLambda, new LambdaIntegrationOptions
        {
            RequestTemplates = new Dictionary<string, string> { ["application/json"] = "{ \"statusCode\": \"200\" }" }
        });

        var telegramPath = restApi.Root.AddResource("telegram");

        telegramPath.AddProxy(new ProxyResourceOptions
        {
            DefaultIntegration = telegramLambdaIntegration
        });
    }
}