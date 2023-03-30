﻿using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;

namespace CacheUsageDemo.CacheService
{
    public class CacheDistributedService<T> : ACacheService<T>, ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheDistributedService( IDistributedCache cache )
        {
            _cache = cache;
        }

        public async Task PutRecordToCacheAsync( object record )
        {
            DistributedCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds( MillisecondsAbsoluteExpiration )
            };

            T tRecord = (T) record;
            string json = JsonSerializer.Serialize( tRecord );
            byte[] bytes = Encoding.UTF8.GetBytes( json );

            await _cache.SetAsync( tRecord.ToString(), bytes, options );
        }

        public async Task TryToCathRecordPreemptionAsync( object record )
        {
            T? tRecord = default( T );
            byte[]? bytes = await _cache.GetAsync( record.ToString() );

            if ( bytes is { Length: > 0 } )
            {
                string json = Encoding.UTF8.GetString( bytes );
                tRecord = JsonSerializer.Deserialize<T>( json );
            }
        }
    }
}
