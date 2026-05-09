# TPJ.Logging

`TPJ.Logging` is a simple error logging package for .NET applications. It can log errors to:

- a text file
- email
- or both

The package registers `IErrorLogger` with dependency injection so it can be used in console apps, ASP.NET Core APIs, and other .NET applications.

## Install

```bash
dotnet add package TPJ.Logging
```

## Basic configuration

The package reads its settings from configuration.

### `appsettings.json`

This is the simplest setup using file logging:

```json
{
  "TPJ": {
    "Logging": {
      "ApplicationName": "Sample App",
      "Error": {
        "LogType": "LogFile",
        "LogFileDirectory": "C:\\Logs\\TPJ"
      }
    }
  }
}
```

### Available log types

- `Email`
- `LogFile`
- `EmailLogFile`

If you use `Email` or `EmailLogFile`, also configure:

- `TPJ:Logging:Error:Email:From`
- `TPJ:Logging:Error:Email:To`

and the required `TPJ.Email` settings for sending email.

## Console app example

For a console app, the easiest approach is to use `Host.CreateApplicationBuilder` so configuration and dependency injection are available.

### `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TPJ.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration["ASPNETCORE_ENVIRONMENT"] ??= builder.Environment.EnvironmentName;

builder.Services.AddTPJLogging();
builder.Services.AddTransient<DemoRunner>();

using var host = builder.Build();

await host.Services.GetRequiredService<DemoRunner>().RunAsync();

internal sealed class DemoRunner(IErrorLogger errorLogger)
{
    public async Task RunAsync()
    {
        try
        {
            throw new InvalidOperationException("Something went wrong in the console app.");
        }
        catch (Exception ex)
        {
            await errorLogger.LogAsync(
                MethodBase.GetCurrentMethod()!,
                ex,
                new { Command = "demo" },
                "Console app example");

            Console.WriteLine("The error was logged.");
        }
    }
}
```

If your console app uses `appsettings.json`, make sure the file is copied to the output directory.

## ASP.NET Core API example

Register the package during startup, then inject `IErrorLogger` into a controller or endpoint.

### `Program.cs`

```csharp
using TPJ.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddTPJLogging();

var app = builder.Build();

app.MapControllers();

app.Run();
```

### Example controller

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using TPJ.Logging;

namespace SampleApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController(IErrorLogger errorLogger) : ControllerBase
{
    [HttpGet("error")]
    public async Task<IActionResult> Error()
    {
        try
        {
            throw new InvalidOperationException("Something went wrong in the API.");
        }
        catch (Exception ex)
        {
            await errorLogger.LogAsync(
                MethodBase.GetCurrentMethod()!,
                ex,
                new
                {
                    Path = Request.Path.Value,
                    TraceIdentifier = HttpContext.TraceIdentifier
                },
                "API example");

            return Problem("The error was logged.");
        }
    }
}
```

## Notes

- `IErrorLogger` supports sync and async logging.
- You can pass additional details as an object or array of objects.
- `MethodBase.GetCurrentMethod()` is used so the logger can include method information in the log output.
