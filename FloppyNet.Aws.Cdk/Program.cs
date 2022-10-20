using Amazon.CDK;
using FloppyNet.Aws.Cdk;
using Microsoft.Extensions.Configuration;

var configBuilder = new ConfigurationBuilder();
configBuilder.AddJsonFile("appsettings.json");
configBuilder.AddJsonFile("appsettings.Production.json");

var config = configBuilder.Build();
var app = new App();

new FloppyNetStack(app, config, new StackProps
{
    Env = new Amazon.CDK.Environment { Region = config["Stack:Region"] }
});

var result = app.Synth();