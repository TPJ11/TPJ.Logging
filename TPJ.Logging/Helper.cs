using static TPJ.Logging.Enums;

namespace TPJ.Logging;

public static class Helper
{
    /// <summary>
    /// Gets the error log type from a string
    /// </summary>
    /// <param name="errorLogType">Error log type as a string value</param>
    /// <returns>Error log type as enum</returns>
    public static ErrorLogTypes GetErrorLogType(string errorLogType)
    {
        if (!string.IsNullOrWhiteSpace(errorLogType))
        {
            switch (errorLogType.Trim().ToLower())
            {
                case "email":
                    return ErrorLogTypes.Email;
                case "logfile":
                    return ErrorLogTypes.LogFile;
                case "emaillogfile":
                    return ErrorLogTypes.EmailLogFile;
            }
        }

        throw new Exception("Error Log Type invalid");
    }
}
