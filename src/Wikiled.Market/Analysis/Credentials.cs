using System;
using Tweetinvi;
using Tweetinvi.Models;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        public static string QuandlKey { get; } = Environment.GetEnvironmentVariable("QuandlKey");

        public static ITwitterCredentials TwitterAppCredentials { get; } =
            Auth.SetApplicationOnlyCredentials(Environment.GetEnvironmentVariable("AppKey"), Environment.GetEnvironmentVariable("AppSecret"));

        public static ITwitterCredentials TwitterCredentials { get; } = new TwitterCredentials(
            Environment.GetEnvironmentVariable("CONSUMER_KEY"),
            Environment.GetEnvironmentVariable("CONSUMER_SECRET"),
            Environment.GetEnvironmentVariable("AppKey"),
            Environment.GetEnvironmentVariable("AppSecret"));
    }
}
