FROM microsoft/dotnet:2.1-sdk AS builder
ARG BUILD_VERSION  
WORKDIR /source

COPY . .
RUN dotnet publish /p:Version=$env:BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Wikiled.Market.Console.dll", "bot", "-Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM", "-IsKeyAuth"]

