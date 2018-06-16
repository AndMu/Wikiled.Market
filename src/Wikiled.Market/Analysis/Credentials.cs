using System;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        public static string QuandlKey { get; } = Environment.GetEnvironmentVariable("QUANDL_KEY")?.Trim();

        public static ITwitterCredentials TwitterAppCredentials { get; } =
            Auth.SetApplicationOnlyCredentials(Environment.GetEnvironmentVariable("TW_APP_KEY")?.Trim(), Environment.GetEnvironmentVariable("TW_APP_SECRET")?.Trim());

        public static ITwitterCredentials TwitterCredentials { get; } = new TwitterCredentials(
            Environment.GetEnvironmentVariable("TW_CONSUMER_KEY")?.Trim(),
            Environment.GetEnvironmentVariable("TW_CONSUMER_SECRET")?.Trim(),
            Environment.GetEnvironmentVariable("TW_APP_KEY")?.Trim(),
            Environment.GetEnvironmentVariable("TW_APP_SECRET")?.Trim());
    }
}
