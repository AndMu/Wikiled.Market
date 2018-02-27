using System;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        public static string QuandlKey => Environment.GetEnvironmentVariable("QuandlKey") ?? "YOUR_API_KEY";
    }
}
