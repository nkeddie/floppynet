dotnet lambda package -o output\FloppyNet.Aws.TelegramLambda.zip -pl FloppyNet.Aws.TelegramLambda
Expand-Archive -Force .\output\FloppyNet.Aws.TelegramLambda.zip .\output\FloppyNet.Aws.TelegramLambda

dotnet lambda package -o output\FloppyNet.Aws.ReminderLambda.zip -pl FloppyNet.Aws.ReminderLambda
Expand-Archive -Force .\output\FloppyNet.Aws.ReminderLambda.zip .\output\FloppyNet.Aws.ReminderLambda

dotnet lambda package -o output\FloppyNet.Aws.WordleLambda.zip -pl FloppyNet.Aws.WordleLambda
Expand-Archive -Force .\output\FloppyNet.Aws.WordleLambda.zip .\output\FloppyNet.Aws.WordleLambda

#cdk bootstrap
#cdk deploy