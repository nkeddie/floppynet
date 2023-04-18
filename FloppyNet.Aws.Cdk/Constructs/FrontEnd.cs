using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.Route53;

namespace FloppyNet.Aws.Cdk.Constructs
{
    public record FrontEndProps(IHostedZone HostedZone, string Domain, Certificate Certificate);

    public class FrontEnd : Construct
    {
        public Bucket s3Bucket;

        public FrontEnd(Construct scope, string id, FrontEndProps props) : base(scope, id)
        {
            ArgumentNullException.ThrowIfNull(props.Domain);
            ArgumentNullException.ThrowIfNull(props.Certificate);
            ArgumentNullException.ThrowIfNull(props.HostedZone);

            s3Bucket = new Bucket(this, "WordleBucket", new BucketProps
            {
                AccessControl = BucketAccessControl.PUBLIC_READ,
                BucketName = props.Domain,
                WebsiteIndexDocument = "index.html",
                PublicReadAccess = true
            });

            var cloudFrontOrigin = new S3Origin(s3Bucket);

            var cloudFront = new Distribution(this, "WebDist", new DistributionProps
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
                Certificate = props.Certificate,
                DomainNames = new[] { props.Domain }
            });


            new BucketDeployment(this, "WordleDeployment", new BucketDeploymentProps
            {
                DestinationBucket = s3Bucket,
                Sources = new[] { Source.Asset(Path.Combine("FloppyNet.UI", "floppy-net", "dist")) },
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

            new ARecord(this, "WebRecord", new ARecordProps
            {
                Zone = props.HostedZone,
                RecordName = props.Domain,
                Target = RecordTarget.FromAlias(new CloudFrontTarget(cloudFront))
            });
        }
    }
}
