using Amazon.CDK;
using FloppyNet.Aws.Cdk;
using Microsoft.Extensions.Configuration;

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json");
configBuilder.AddJsonFile("appsettings.Production.json", optional: true);
configBuilder.AddEnvironmentVariables();
var config = configBuilder.Build();
var app = new App();

new FloppyNetStack(app, config, new StackProps
{
    Env = new Amazon.CDK.Environment { Region = config["Stack:Region"], Account = config["Stack:Account"] }
});

var result = app.Synth();