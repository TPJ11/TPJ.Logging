using Microsoft.Extensions.Configuration;
using TPJ.Logging.Enums;

namespace TPJ.Logging;

public interface ILogSettings
{
    ErrorLogTypes LogType { get; set; }
    string LogFileDirectory { get; set; }
    string? Environment { get; set; }
    string EmailFrom { get; set; }
    IEnumerable<string> EmailTo { get; set; }
    string SmtpClient { get; set; }
    string? SmtpUser { get; set; }
    string? SmtpPassword { get; set; }
    int? Port { get; set; }
    bool EnableSSL { get; set; }
    string ApplicationName { get; set; }
}

public class LogSettings : ILogSettings
{
    public ErrorLogTypes LogType { get; set; }
    public string LogFileDirectory { get; set; }
    public string? Environment { get; set; }
    public string EmailFrom { get; set; }
    public IEnumerable<string> EmailTo { get; set; } = new List<string>();
    public string SmtpClient { get; set; }
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public int? Port { get; set; }
    public bool EnableSSL { get; set; }
    public string ApplicationName { get; set; }

    public LogSettings()
    {
    }

    /// <summary>
    /// Load from config file
    /// </summary>
    /// <param name="configuration">Config settings</param>
    public LogSettings(IConfiguration configuration)
    {
        LogType = GetErrorLogType(configuration["TPJ:Logging:LogType"]);

        Environment = configuration["ASPNETCORE_ENVIRONMENT"];
        ApplicationName = configuration["TPJ:Logging:ApplicationName"];

        // Log File Settings
        LogFileDirectory = string.IsNullOrWhiteSpace(configuration["TPJ:Logging:LogFileDirectory"]) ? null : configuration["TPJ:Logging:LogFileDirectory"];

        // E-mail Settings
        EmailTo = string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:To"]) ? null : configuration["TPJ:Logging:Email:To"].Split(';');

        EmailFrom = !string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:From"]) ? configuration["TPJ:Logging:Email:From"] :
                        !string.IsNullOrWhiteSpace(configuration["TPJ:Email:From"]) ? configuration["TPJ:Email:From"] : null;

        SmtpClient = !string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:SmtpClient"]) ? configuration["TPJ:Logging:Email:SmtpClient"] :
                        !string.IsNullOrWhiteSpace(configuration["TPJ:Email:SmtpClient"]) ? configuration["TPJ:Email:SmtpClient"] : null;

        SmtpUser = !string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:SmtpUser"]) ? configuration["TPJ:Logging:Email:SmtpUser"] :
                        !string.IsNullOrWhiteSpace(configuration["TPJ:Email:SmtpUser"]) ? configuration["TPJ:Email:SmtpUser"] : null;

        SmtpPassword = !string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:SmtpPassword"]) ? configuration["TPJ:Logging:Email:SmtpPassword"] :
                        !string.IsNullOrWhiteSpace(configuration["TPJ:Email:SmtpPassword"]) ? configuration["TPJ:Email:SmtpPassword"] : null;

        if (!string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:Port"]))
        {
            if (int.TryParse(configuration["TPJ:Logging:Email:Port"], out var portNumber))
                Port = portNumber;
        }
        else if (!string.IsNullOrWhiteSpace(configuration["TPJ:Email:Port"]))
        {
            if (int.TryParse(configuration["TPJ:Email:Port"], out var portNumber))
                Port = portNumber;
        }

        if (!string.IsNullOrWhiteSpace(configuration["TPJ:Logging:Email:EnableSSL"]))
        {
            if (bool.TryParse(configuration["TPJ:Logging:Email:EnableSSL"], out var enableSSL))
                EnableSSL = enableSSL;
        }
        else if (!string.IsNullOrWhiteSpace(configuration["TPJ:Email:EnableSSL"]))
        {
            if (bool.TryParse(configuration["TPJ:Email:EnableSSL"], out var enableSSL))
                EnableSSL = enableSSL;
        }

        //Check all the required stuff is passed in
        if (LogType == ErrorLogTypes.Email)
        {
            if (string.IsNullOrWhiteSpace(ApplicationName) || string.IsNullOrWhiteSpace(SmtpClient)
                || string.IsNullOrWhiteSpace(EmailFrom) || !EmailTo.Any())
                throw new Exception("Application Name, SMTP Client, E-mail From, E-mail To, and Enable SSL Required!");
        }
        else if (LogType == ErrorLogTypes.LogFile)
        {
            if (string.IsNullOrWhiteSpace(LogFileDirectory) || string.IsNullOrWhiteSpace(ApplicationName))
                throw new Exception("Log File Directory and Application Name Required!");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(LogFileDirectory) || string.IsNullOrWhiteSpace(ApplicationName)
                || string.IsNullOrWhiteSpace(ApplicationName) || string.IsNullOrWhiteSpace(SmtpClient)
                || string.IsNullOrWhiteSpace(EmailFrom) || !EmailTo.Any())
                throw new Exception("Application Name, SMTP Client, E-mail From, E-mail To, Enable SSL, Log File Directory, and Application Name Required!");
        }
    }

    /// <summary>
    /// Gets the error log type from a string
    /// </summary>
    /// <param name="logType">Error log type as a string value</param>
    /// <returns>Error log type as enum</returns>
    public static ErrorLogTypes GetErrorLogType(string? logType)
    {
        if (!string.IsNullOrWhiteSpace(logType))
        {
            if (logType.Equals("email", StringComparison.OrdinalIgnoreCase)) 
                return ErrorLogTypes.Email;

            if (logType.Equals("logfile", StringComparison.OrdinalIgnoreCase)) 
                return ErrorLogTypes.LogFile;

            if (logType.Equals("emaillogfile", StringComparison.OrdinalIgnoreCase)) 
                return ErrorLogTypes.EmailLogFile;
        }

        throw new Exception("Error Log Type invalid");
    }
}
