$Region = [System.Environment]::GetEnvironmentVariable('Stack__Region')
$Account = [System.Environment]::GetEnvironmentVariable('Stack__Account')

if ($Region -eq $null) { throw "Region is not defined" }
if ($Account -eq $null) { throw "Account is not defined" }

cdk bootstrap "$Account/$Region"
cdk deploy