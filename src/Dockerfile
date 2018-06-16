FROM microsoft/dotnet:2.1-sdk AS builder
ARG BUILD_VERSION  
ARG APP_KEY  
ARG APP_SECRET 
ARG QUANDL_KEY
ARG CONSUMER_KEY
ARG CONSUMER_SECRET
WORKDIR /source

# copies the rest of your code
COPY . .
RUN dotnet publish /p:Version=$env:BUILD_VERSION --output /app/ --configuration Release

# Stage 2
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
ENV QuandlKey $QUANDL_KEY
ENV AppKey $APP_KEY
ENV AppSecret $APP_SECRET
ENV CONSUMER_KEY $CONSUMER_KEY
ENV CONSUMER_SECRET $CONSUMER_SECRET

ENTRYPOINT ["dotnet", "Wikiled.Market.Console.dll", "bot", "-Stocks=AMD,GOOG,FB,MMM,CAT,AMZN,AXP,KO,INTC,PM,TIF,WFC,MS,JPM", "-IsKeyAuth"]

