using CacheUsageDemo.CacheService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static IHost BuildHost( bool isInMemmory )
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices( services => {
                if ( isInMemmory )
                {
                    services.AddMemoryCache();
                    services.AddScoped<ICacheService, CacheInMemmoryService<string>>();
                } 
                else
                {
                    services.AddDistributedMemoryCache();
                    services.AddScoped<ICacheService, CacheDistributedService<string>>();
                }
            } )
            .Build();

        return host;
    }


    static async ValueTask IterateAssAsync( Func<string, Task> asyncFunc )
    {
        for ( int i = 1; i < 30; i++ )
        {
            await asyncFunc( $"ЖОПА_{i}" );
        }

        Console.WriteLine();
    }

    public static async Task<int> Main( string[] args )
    {
        try
        {
            bool isInMemmory = args.Length > 0 ? args[0] == "in-memmory" ? true : false : false;
            IHost host = BuildHost( isInMemmory );
            ICacheService cacheService = host.Services.GetRequiredService<ICacheService>();

            var addAssesToCacheTask = IterateAssAsync( cacheService.PutRecordToCacheAsync );
            await addAssesToCacheTask;

            var readAssesFromCacheTask = IterateAssAsync( cacheService.TryToCathRecordPreemptionAsync );
            await readAssesFromCacheTask;

            return 0;
        } catch ( Exception ex )
        {
            Console.WriteLine( ex );
        }

        return 1;
    }
}