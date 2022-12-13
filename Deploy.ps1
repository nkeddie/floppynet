$Region = [System.Environment]::GetEnvironmentVariable('Stack__Region')
$Account = [System.Environment]::GetEnvironmentVariable('Stack__Account')

if ($Region -eq $null) { throw "Region is not defined" }
if ($Account -eq $null) { throw "Account is not defined" }

# Package .NET 7 lambda into output directory
dotnet lambda package -pl .\FloppyNet.Aws.ReminderLambda -o .\output\FloppyNet.Aws.ReminderLambda.zip

# Required to pull image from public repository
aws ecr-public get-login-password --region $Region | docker login --username AWS --password-stdin public.ecr.aws

cdk bootstrap "$Account/$Region"
cdk deploy