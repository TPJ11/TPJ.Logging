using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TPJ.Logging;

public static class DependencyInjection
{
    public static void AddTPJLogging(this IServiceCollection services)
    {
        services.TryAddSingleton<ILogSettings, LogSettings>();
        services.TryAddSingleton<ILogger, Logger>();
    }
}
