using Microsoft.Extensions.Caching.Memory;

namespace CacheUsageDemo.CacheService
{
    public abstract class ACacheService<T>
    {
        protected readonly int MillisecondsDelayAfterAdd = 50;
        protected readonly int MillisecondsAbsoluteExpiration = 750;

        public void OnPostEviction( object key, object? record, EvictionReason reason, object? state )
        {
            if ( record is T tObject )
            {
                Console.WriteLine( $"{tObject.ToString()} was evicted for {reason}." );
            }
        }
    }
}
