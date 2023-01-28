namespace TPJ.Logging.Enums;

/// <summary>
/// Type of logs that can be made for an error
/// Email - Only an E-mail will be sent
/// LogFile - Details of the error will only be added to the log file
/// EmailLogFile - Both an e-mail will be sent and the details added to the log file
/// </summary>
public enum ErrorLogTypes
{
    Email,
    LogFile,
    EmailLogFile
}