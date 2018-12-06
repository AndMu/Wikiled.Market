FROM microsoft/dotnet:2.2.100-sdk AS builder
ARG BUILD_VERSION  
WORKDIR /source

COPY . .
RUN dotnet publish /p:Version=$BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.2.0-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Wikiled.Market.Console.dll", "bot", "-IsService"]

