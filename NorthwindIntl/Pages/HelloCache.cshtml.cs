using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace NorthwindIntl.Pages
{
    public class HelloCachePageModel : PageModel
    {
        private readonly IMemoryCache _cache;
        public const string CancellationTokenSourceName = "cancellationTokenSource";

        public HelloCachePageModel(IMemoryCache cache)
        {
            _cache = cache;
        }
        public void OnGet()
        {

        }
        public void OnGetCreateDependentCache()
        {
            var cts = new CancellationTokenSource();
            _cache.Set(CancellationTokenSourceName, cts);
            using (var parentItem = _cache.CreateEntry("parent"))// using this pattern we can create cache item which have similar cache entry option with the using source (parentItem)
            {
                var changeToken = new CancellationChangeToken(cts.Token); // we can use this even after cancellation to generate a new token and so we can cancel it again!
                parentItem.Value = DateTime.Now;
                // parentItem.ExpirationTokens.Add(new CancellationChangeToken(cts.Token));// This does not need, all the property share here between the entries
                _cache.Set("child", DateTime.UtcNow, changeToken); // we related Item using a created change token which itself store in cache too (Or we can save it in memory if we can!)
                foreach (var item in parentItem.ExpirationTokens)
                {
                    Console.WriteLine(item.GetType());// Check that cancellation Token was write for parent too (Since we are use using block with patent entry, all the option for child elements inside the block overright for parent too but the inverse is not true!)
                }
                _cache.Set("daughter", DateTime.Now, changeToken);
                _cache.Set("son", DateTime.Now, changeToken);
            }
        }

        public void OnGetRemoveDependentCache()
        {
            var cts = _cache.Get<CancellationTokenSource>(CancellationTokenSourceName);
            cts.Cancel();// This remove all the dependent entries on cache but in other thread so the result may not be observed just after calling the cancel
        }
    }
}

// Some Not about cache
// Expiration doesn't happen in the background. There's no timer that actively scans the cache for expired items.
// Any activity on the cache (Get, Set, Remove) can trigger a background scan for expired items.
// Cancellation of source of change Token/ Expiration Token also expires the entry and triggers a scan for expired items. (This is happen since the cancellation run on another thread)
// When this token fires, it removes the entry immediately and fires the eviction callbacks.
// Use a background service such as IHostedService to update the cache. The background service can recompute the entries and then assign them to the cache only when they’re ready.