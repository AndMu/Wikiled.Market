using System;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        public static string QuandlKey { get; } = Environment.GetEnvironmentVariable("QuandlKey")?.Trim();

        public static ITwitterCredentials TwitterAppCredentials { get; } =
            Auth.SetApplicationOnlyCredentials(Environment.GetEnvironmentVariable("AppKey")?.Trim(), Environment.GetEnvironmentVariable("AppSecret")?.Trim());

        public static ITwitterCredentials TwitterCredentials { get; } = new TwitterCredentials(
            Environment.GetEnvironmentVariable("CONSUMER_KEY")?.Trim(),
            Environment.GetEnvironmentVariable("CONSUMER_SECRET")?.Trim(),
            Environment.GetEnvironmentVariable("APP_KEY")?.Trim(),
            Environment.GetEnvironmentVariable("APP_SECRET")?.Trim());
    }
}
