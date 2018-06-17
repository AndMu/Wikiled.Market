using System;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        public static string QuandlKey { get; } = GetEnvironmentVariable("QUANDL_KEY")?.Trim();

        public static ITwitterCredentials TwitterAppCredentials { get; } =
            Auth.SetApplicationOnlyCredentials(GetEnvironmentVariable("TW_APP_KEY")?.Trim(), GetEnvironmentVariable("TW_APP_SECRET")?.Trim());

        public static ITwitterCredentials TwitterCredentials { get; } = new TwitterCredentials(
            GetEnvironmentVariable("TW_CONSUMER_KEY")?.Trim(),
            GetEnvironmentVariable("TW_CONSUMER_SECRET")?.Trim(),
            GetEnvironmentVariable("TW_APP_KEY")?.Trim(),
            GetEnvironmentVariable("TW_APP_SECRET")?.Trim());

        private static string GetEnvironmentVariable(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (value != null)
            {
                return value;
            }

            value = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);
            if (value != null)
            {
                return value;
            }

            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
        }
    }
}
