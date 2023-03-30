namespace CacheUsageDemo.CacheService
{
    public interface ICacheService
    {
        // Добавляем запись в кэш
        Task PutRecordToCacheAsync( object record );

        // Пытаемся поймать событие вытеснения записи
        Task TryToCathRecordPreemptionAsync( object record );
    }
}
