using System;

namespace Wikiled.Market.Analysis
{
    public static class Credentials
    {
        // git update-index --assume-unchanged Credentials.cs
        // git update-index --no-assume-unchanged Credentials.cs
        // git ls-files -v|grep '^h'

        public static string QuandlKey => Environment.GetEnvironmentVariable("QuandlKey") ?? "YOUR_API_KEY";
    }
}
