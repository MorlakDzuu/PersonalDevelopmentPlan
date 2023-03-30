using Microsoft.Extensions.Caching.Memory;

namespace CacheUsageDemo.CacheService
{
    public class CacheInMemmoryService<T> : ACacheService<T>, ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheInMemmoryService( IMemoryCache cache )
        {
            _cache = cache;
        }

        public Task PutRecordToCacheAsync( object record )
        {
            MemoryCacheEntryOptions options = new()
            {
                // Задаем время время жизни записи
                AbsoluteExpirationRelativeToNow =
            TimeSpan.FromMilliseconds( MillisecondsAbsoluteExpiration )
            };

            // Задаем обратный вызов
            _ = options.RegisterPostEvictionCallback( OnPostEviction );

            // Вызываем метод Set, принадлежащий API экземпляра IMemoryCache
            T recordInst = _cache.Set( record, ( T ) record, options );

            // Фиксируем действие добавления записи в кэш
            Console.WriteLine( $"{recordInst} was cached." );

            // Создаем задачу и откладываем ее выполнение на определенное время
            return Task.Delay( TimeSpan.FromMilliseconds( MillisecondsDelayAfterAdd ) );
        }

        public Task TryToCathRecordPreemptionAsync( object record )
        {
            if ( _cache.TryGetValue( record, out object? value ) && value is T tObj )
            {
                Console.WriteLine( $"{record} is still in cache. {tObj}" );
            }

            return Task.CompletedTask;
        }
    }
}
