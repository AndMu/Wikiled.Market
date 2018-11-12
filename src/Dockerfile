FROM microsoft/dotnet:2.1.403-sdk AS builder
ARG BUILD_VERSION  
WORKDIR /source

COPY . .
RUN dotnet publish /p:Version=$BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.1.5-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Wikiled.Market.Console.dll", "bot", "-Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,INTC,PM,MS,JPM,MU,TSLA", "-IsService"]

