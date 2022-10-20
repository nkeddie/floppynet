FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as build
WORKDIR /src
COPY ["FloppyNet.Aws.TelegramLambda.csproj", "FloppyNet.Aws.TelegramLambda/"]
RUN dotnet restore "FloppyNet.Aws.TelegramLambda/FloppyNet.Aws.TelegramLambda.csproj"

WORKDIR "/src/FloppyNet.Aws.TelegramLambda"
COPY . .
RUN dotnet build "FloppyNet.Aws.TelegramLambda.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "FloppyNet.Aws.TelegramLambda.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
CMD ["FloppyNet.Aws.TelegramLambda::FloppyNet.Aws.TelegramLambda.Function::FunctionHandler"]