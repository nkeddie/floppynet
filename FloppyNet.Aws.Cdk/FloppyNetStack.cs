using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Route53;
using FloppyNet.Aws.Cdk.Constructs;
using Microsoft.Extensions.Configuration;

namespace FloppyNet.Aws.Cdk;

public class FloppyNetStack : Stack
{
    public FloppyNetStack(Construct scope, IConfiguration config, IStackProps? props = null) : base(scope, config["Stack:Name"], props)
    {
        string domain = config["Stack:DomainRoot"]!;
        string hostedZoneId = config["Stack:HostedZoneId"]!;

        var hostedZone = HostedZone.FromHostedZoneAttributes(this, "FloppyNetHostedZone", new HostedZoneAttributes
        {
            HostedZoneId = hostedZoneId,
            ZoneName = domain
        });

        var certificate = new Certificate(this, "FloppyNetApiCert", new CertificateProps
        {
            DomainName = $"*.{domain}",
            SubjectAlternativeNames = new[] { domain },
            Validation = CertificateValidation.FromDns(hostedZone)
        });

        var telegramLambda = new TelegramLambda(this, "Telegram", new TelegramLambdaProps(config["Telegram:SecretKey"]!));
        var wordleLambda = new WordleLambda(this, "Wordle", new WordleLambdaProps(telegramLambda.Queue));
        new ReminderLambda(this, "Reminder", new ReminderLambdaProps(domain, config["Telegram:BotCredentials"]!, config["Telegram:ChatId"]!));

        var frontEnd = new FrontEnd(this, "FrontEnd", new FrontEndProps(hostedZone, domain, certificate));
        frontEnd.s3Bucket.GrantReadWrite(wordleLambda.Lambda);
        frontEnd.s3Bucket.GrantPutAcl(wordleLambda.Lambda);

        var api = new Api(this, "Api", new ApiProps(hostedZone, domain, certificate, telegramLambda.Lambda));
    }
}