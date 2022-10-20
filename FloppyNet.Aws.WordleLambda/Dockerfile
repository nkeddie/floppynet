FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as build
WORKDIR /src
COPY ["FloppyNet.Aws.WordleLambda.csproj", "FloppyNet.Aws.WordleLambda/"]
RUN dotnet restore "FloppyNet.Aws.WordleLambda/FloppyNet.Aws.WordleLambda.csproj"

WORKDIR "/src/FloppyNet.Aws.WordleLambda"
COPY . .
RUN dotnet build "FloppyNet.Aws.WordleLambda.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "FloppyNet.Aws.WordleLambda.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
CMD ["FloppyNet.Aws.WordleLambda::FloppyNet.Aws.WordleLambda.Function::FunctionHandler"]