using System;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using Wikiled.Text.Analysis.Twitter;

namespace Wikiled.Market.Sentiment
{
    public class DublicateDetectors
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IMemoryCache cache;

        private readonly MessageCleanup cleanup = new MessageCleanup();

        public DublicateDetectors(IMemoryCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public bool HasReceived(string text)
        {
            text = cleanup.Cleanup(text);
            if (cache.TryGetValue(text, out bool _))
            {
                log.Debug("Found dublicate: {0}", text);
                return true;
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20));
            cache.Set(text, true, cacheEntryOptions);
            return false;
        }
    }
}
