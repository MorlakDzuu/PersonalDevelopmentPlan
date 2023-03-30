# Разобраться с принцыпами кэширования

__Кэширование__ - временное хранение данных в оперативной памяти для оптимизации производительности приложения.
Данные стоит кэшировать, если они изменяются нечасто.

* __Типы кэширования__
    + Кэширование в памяти *(Microsoft.Extensions.Caching.Memory)*
    + Распределенное кэширование *(Microsoft.Extensions.Caching.Distributed)*

 ## __Кэширование в памяти__ ##

Этот тип кэширования подходит для приложений, работающий на одном сервере, так как у них одно адресное пространство, принадлежащее процессу приложения

 __IMemoryCache__ - представляет локальный кэш в памяти, его значения не сериализуется. Его текущая реализация - оболочка ConcurrentDictionary<TKey,TValue>

 __ICacheEntry__ - представляет запись в реализации кэша IMemoryCache, может быть любым __object__

Потребитель кэша может задать или изменить время жизни записи в кэшэ, так как имеет доступ к следующим
* свойствам:
    + __ICacheEntry.AbsoluteExpiration__ - абсолютная дата окончания срока готдности (принимает DateTimeOffset)
    + __ICacheEntry.AbsoluteExpirationRelativeToNow__ - абсолютная дата окончания срока годонсти относительно текущего момента (принимает TimeSpan?)
    + __ICacheEntry.SlidingExpiration__ - допустимое время неактивности записи
 
 __!!Если запись протухнет, то она вытесняется__

 * Потребитель может настроить поведение записей с помощью __MemoryCacheEntryOptions__, например:
    + __MemoryCacheEntryOptions__ позволяет задать срок действия и обратный вызов при изменении записи 
        - *MemoryCacheEntryExtensions.AddExpirationToken* - срок действия записи = срок действия токена 
        - *MemoryCacheEntryExtensions.RegisterPostEvictionCallback* - задает обратный вызов
    + __CacheItemPriority__ позволяет настроить приоритеты записей (какую запись можно удалить в случае нехватки памяти)
        - *MemoryCacheEntryExtensions.SetPriority* задает приоритет записи
    + __ICacheEntry.Size__ позволяет задать или изменить размер записи в кэшэ
        - *MemoryCacheEntryExtensions.SetSize* задает размер записи

### __Реализация__ ###

1. Прописать __AddMemoryCache()__ в конфигурации сервисов
```csharp
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddMemoryCache())
    .Build();
```

2. Получить инстанс __IMemoryCache__, например
```csharp
IMemoryCache cache = host.Services.GetRequiredService<IMemoryCache>();
```

3. Реализовать запись в кэш, например закинем в кэш буковки
```csharp

// Поведение при добавлении записи в кэш
var addLettersToCacheTask = IterateAlphabetAsync(letter =>
{
    MemoryCacheEntryOptions options = new()
    {
        // Задаем время время жизни записи
        AbsoluteExpirationRelativeToNow =
            TimeSpan.FromMilliseconds(MillisecondsAbsoluteExpiration)
    };

    // Задаем обратный вызов
    _ = options.RegisterPostEvictionCallback(OnPostEviction);

    // Вызываем метод Set, принадлежащий API экземпляра IMemoryCache
    AlphabetLetter alphabetLetter =
        cache.Set(
            letter, new AlphabetLetter(letter), options);

    // Фиксируем действие добавления записи в кэш
    Console.WriteLine($"{alphabetLetter.Letter} was cached.");

    // Создаем задачу и откладываем ее выполнение на определенное время
    return Task.Delay(
        TimeSpan.FromMilliseconds(MillisecondsDelayAfterAdd));
});
await addLettersToCacheTask;

// Подписываемся на срабатывание обратного вызова
var readLettersFromCacheTask = IterateAlphabetAsync(letter =>
{
    // Проверяем сработал ли обратный вызов
    if (cache.TryGetValue(letter, out object? value) &&
        value is AlphabetLetter alphabetLetter)
    {
        // Обратный вызов еще не сработал, значит запись еще в кэше
        Console.WriteLine($"{letter} is still in cache. {alphabetLetter.Message}");
    }

    return Task.CompletedTask;
});
await readLettersFromCacheTask;
```

 ## __Распределенное кэширование__ ##

 API распределенного кэширования немного проще, чем их аналоги API кэширования в памяти. Пары "ключ — значение" тоже проще. Ключи кэширования в памяти основаны на object, а ключи распределенного кэширования являются типом string. При использовании кэширования в памяти значением может быть любой строго типизированный универсальный тип, тогда как значения в распределенном кэшировании сохраняются в виде byte[]. Это не означает, что различные реализации не предоставляют строго типизированные универсальные значения, но это будет особенностью реализации.

 ### __Реализация__ ###
1. Вместо __AddMemoryCache()__ прописать в конфигурации сервисов __AddDistributedMemoryCache()__
```csharp
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddDistributedMemoryCache())
    .Build();
```

2. Создание значений
```csharp
DistributedCacheEntryOptions options = new()
{
    // Задаем время жизни записи
    AbsoluteExpirationRelativeToNow =
        TimeSpan.FromMilliseconds(MillisecondsAbsoluteExpiration)
};

// Сериализуем запись json строку
AlphabetLetter alphabetLetter = new(letter);
string json = JsonSerializer.Serialize(alphabetLetter);
byte[] bytes = Encoding.UTF8.GetBytes(json);

// Добавляем в кэш
await cache.SetAsync(letter.ToString(), bytes, options);
```

3. Чтение значений
```csharp
AlphabetLetter? alphabetLetter = null;

// Пробуем прочитать из кеша
byte[]? bytes = await cache.GetAsync(letter.ToString());

// Проверяем прочитали ли
if (bytes is { Length: > 0 })
{
    // Десериализуем
    string json = Encoding.UTF8.GetString(bytes);
    alphabetLetter = JsonSerializer.Deserialize<AlphabetLetter>(json);
}
```
