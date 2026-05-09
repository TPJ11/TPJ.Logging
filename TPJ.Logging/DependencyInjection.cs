using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TPJ.Email;
using TPJ.Logging.Models;

namespace TPJ.Logging;

public static class DependencyInjection
{
    public static void AddTPJLogging(this IServiceCollection services)
    {
        services.AddTPJEmail();
        services.TryAddSingleton<IErrorLogSettings, ErrorLogSettings>();
        services.TryAddSingleton<IErrorLogger, ErrorLogger>();
    }
}
