using Microsoft.Extensions.Configuration;
using static TPJ.Logging.Enums;

namespace TPJ.Logging.Models;

public interface IErrorLogSettings
{
    ErrorLogTypes LogType { get; }
    string? LogFileDirectory { get; }
    string Environment { get; }
    string? EmailFrom { get; }
    IEnumerable<string> EmailTo { get; }
    string ApplicationName { get; }
}

	public class ErrorLogSettings : IErrorLogSettings
	{
    public required ErrorLogTypes LogType { get; set; }
    public string? LogFileDirectory { get; set; }
    public required string Environment { get; set; }
    public string? EmailFrom { get; set; }
    public required IEnumerable<string> EmailTo { get; set; }
    public required string ApplicationName { get; set; }

    public ErrorLogSettings()
    {
    }

    public ErrorLogSettings(IConfiguration configuration)
    {
        LogType = Helper.GetErrorLogType(configuration["TPJ:Logging:Error:LogType"]!);
        Environment = configuration["ASPNETCORE_ENVIRONMENT"]!;
        ApplicationName = configuration["TPJ:Logging:ApplicationName"]!;

        // If the 'E-mail To' configuration setting contains {UserName} then we replace that part with the current users name 
        // E.G. {UserName}@gmail.com would change to bill.bob@gmail.com if I (bill bob) 
        // was running it. 
        if (!string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Error:Email:To"])
            && configuration["TPJ:Logging:Error:Email:To"]!.Contains("{UserName}"))
        {
            configuration["TPJ:Logging:Error:Email:To"] = configuration["TPJ:Logging:Error:Email:To"]!
                .Replace("{UserName}", configuration["USERNAME"]);
        }

        // Log File Settings
        LogFileDirectory = configuration["TPJ:Logging:Error:LogFileDirectory"];

        // E-mail Settings
        EmailTo = configuration["TPJ:Logging:Error:Email:To"]!.Split(';') ?? [];

        EmailFrom = !string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Error:Email:From"]) ? configuration["TPJ:Logging:Error:Email:From"] :
                        !string.IsNullOrWhiteSpace(configuration["TPJ:Email:From"]) ? configuration["TPJ:Email:From"] : null;

        //Check all the required stuff is passed in
        if (LogType == ErrorLogTypes.Email)
        {
            if (string.IsNullOrWhiteSpace(ApplicationName)
                || string.IsNullOrWhiteSpace(EmailFrom) 
                || !EmailTo.Any())
                throw new Exception("Application Name,E-mail From, and E-mail To Required!");
        }
        else if (LogType == ErrorLogTypes.LogFile)
        {
            if (string.IsNullOrWhiteSpace(LogFileDirectory) 
                || string.IsNullOrWhiteSpace(ApplicationName))
                throw new Exception("Log File Directory and Application Name Required!");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(LogFileDirectory) 
                || string.IsNullOrWhiteSpace(ApplicationName)
                || string.IsNullOrWhiteSpace(ApplicationName)
                || string.IsNullOrWhiteSpace(EmailFrom) || !EmailTo.Any())
                throw new Exception("Application Name,E-mail From, E-mail To, Log File Directory, and Application Name Required!");
        }
    }
}
