using System.Net.Mail;
using System.Reflection;
using TPJ.Logging.Enums;

namespace TPJ.Logging;

public interface ILogger
{
    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    void Log(MethodBase? methodDetails, Exception exception);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    void Log(MethodBase? methodDetails, Exception exception, string note);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    void Log<T>(MethodBase? methodDetails, Exception exception, T details);

    void Log<T>(MethodBase? methodDetails, Exception exception, T[] details);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    void Log<T>(MethodBase? methodDetails, Exception exception, T details, string note);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    Task LogAsync(MethodBase? methodDetails, Exception exception);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    Task LogAsync(MethodBase? methodDetails, Exception exception, string note);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    Task LogAsync<T>(MethodBase? methodDetails, Exception exception, T details);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    Task LogAsync<T>(MethodBase? methodDetails, Exception exception, T details, string note);
}

public class Logger : ILogger
{
    private readonly ILogSettings _errorSettings;
    private readonly SmtpClient _smtpClient;

    public Logger(ILogSettings errorSettings)
    {
        _errorSettings = errorSettings;

        _smtpClient = new SmtpClient(_errorSettings.SmtpClient)
        {
            EnableSsl = _errorSettings.EnableSSL,
            Port = _errorSettings.Port ?? 25,
        };

        if (!string.IsNullOrWhiteSpace(_errorSettings.SmtpUser)
            && !string.IsNullOrWhiteSpace(_errorSettings.SmtpPassword))
        {
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.Credentials = new System.Net.NetworkCredential(_errorSettings.SmtpUser, _errorSettings.SmtpPassword);
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        }
    }

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    public void Log(MethodBase? methodDetails, Exception exception) => LogError(methodDetails, exception);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    public void Log(MethodBase? methodDetails, Exception exception, string note) => LogError(methodDetails, exception, null, note);

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    public void Log<T>(MethodBase? methodDetails, Exception exception, T details)
    {
        var objectDetails = new List<ErrorDetailItem>();

        if (details is not null)
        {
            var (name, errorDetails) = ObjectDetails.Get(details);
            objectDetails.Add(new ErrorDetailItem(name, errorDetails));
        }

        LogError(methodDetails, exception, objectDetails);
    }

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object array and reads all properties and its details to create a error detail model</param>
    public void Log<T>(MethodBase? methodDetails, Exception exception, T[] details)
    {
        var objectDetails = new List<ErrorDetailItem>();

        if (details is not null)
        {
            foreach (var item in details)
            {
                var (name, errorDetails) = ObjectDetails.Get(item);
                objectDetails.Add(new ErrorDetailItem(name, errorDetails));
            }
        }

        LogError(methodDetails, exception, objectDetails);
    }

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    public void Log<T>(MethodBase? methodDetails, Exception exception, T details, string note)
    {
        var objectDetails = new List<ErrorDetailItem>();

        if (details is not null)
        {
            var (name, errorDetails) = ObjectDetails.Get(details);
            objectDetails.Add(new ErrorDetailItem(name, errorDetails));
        }

        LogError(methodDetails, exception, objectDetails, note);
    }

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    public Task LogAsync(MethodBase? methodDetails, Exception exception) => Task.Run(() => { Log(methodDetails, exception); });

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    public Task LogAsync(MethodBase? methodDetails, Exception exception, string note) => Task.Run(() => { Log(methodDetails, exception, note); });

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    public Task LogAsync<T>(MethodBase? methodDetails, Exception exception, T details) => Task.Run(() => { Log(methodDetails, exception, details); });

    /// <summary>
    /// Log an error getting details from 
    /// the method base and the exception
    /// </summary>
    /// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
    /// <param name="note">Commonly used to add a meaningful description to the error</param>
    public Task LogAsync<T>(MethodBase? methodDetails, Exception exception, T details, string note) => Task.Run(() => { Log(methodDetails, exception, details, note); });

    /// <summary>
    /// Log the error using the passed in 
    /// method details, error exception and user details
    /// </summary>
    /// <param name="methodDetails">Method Details</param>
    /// <param name="exception">Exception</param>
    /// <param name="details">User Details</param>
    private void LogError(MethodBase? methodDetails, Exception exception, List<ErrorDetailItem>? details = null, string? note = null)
    {
        string emailMessageBody = $"<b>Date / Time</b>: {DateTime.Now} <br /><br />" +
                            $"<b>Namespace</b>: {methodDetails?.DeclaringType?.FullName} <br /><br />" +
                            $"<b>Method Name</b>: {methodDetails?.Name} <br /><br />" +
                            $"<b>Exception Type</b>: {exception.GetType()?.Name} <br /><br />";

        string logMessageBody = $"Date / Time: {DateTime.Now}{Environment.NewLine}" +
                $"Namespace: {methodDetails?.DeclaringType?.FullName}{Environment.NewLine}" +
                $"Method name: {methodDetails?.Name}{Environment.NewLine}" +
                $"Exception Type: {exception.GetType()?.Name}{Environment.NewLine}";

        if (!string.IsNullOrWhiteSpace(note))
        {
            emailMessageBody += $"<b>Note</b>: {note} <br /><br />";
            logMessageBody += $"Note: {note}{Environment.NewLine}";
        }

        emailMessageBody += $"<b>Error Message</b>: {exception?.Message} <br /><br />" +
                $"<b>Inner Exception</b>: {exception?.InnerException?.Message} <br /><br />" +
                $"<b>Stack Trace</b>: {exception?.StackTrace}<br /><br />";

        logMessageBody += $"Error Message: {exception?.Message}{Environment.NewLine}" +
                $"Inner Exception: {exception?.InnerException?.Message}{Environment.NewLine}" +
                $"Stack Trace: {exception?.StackTrace}";

        // Put the details model into the e-mail and log message
        if (details?.Count > 0)
        {
            emailMessageBody += $"<br /><b>Details</b> <br />";
            logMessageBody += $"{Environment.NewLine}Details";

            var isFirst = true;

            foreach (var item in details)
            {
                var (email, log) = AddDetails(item.ErrorDetails);

                emailMessageBody += isFirst ? $"<b>{item.Name}</b>" : $"<br /><br /><b>{item.Name}</b>";
                logMessageBody += $"{Environment.NewLine}{item.Name}";

                emailMessageBody += email;
                logMessageBody += log;

                isFirst = false;
            }               
        }

        if (_errorSettings.LogType == ErrorLogTypes.Email || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
        {
            var emailTemplate = EmailTemplate.Template();

            emailTemplate = emailTemplate.Replace("@Model.Title", string.IsNullOrWhiteSpace(_errorSettings.Environment) ? _errorSettings.ApplicationName : $"[{_errorSettings.Environment}] {_errorSettings.ApplicationName}");

            emailTemplate = emailTemplate.Replace("@Model.SecondHeader", "Error");
            emailTemplate = emailTemplate.Replace("@Model.Body", emailMessageBody);

            SendEmail(emailTemplate, $"{_errorSettings.ApplicationName} Error", _errorSettings.EmailTo, _errorSettings.EmailFrom, true);
        }

        if (_errorSettings.LogType == ErrorLogTypes.LogFile || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
            CreateErrorLog(Environment.NewLine + logMessageBody + Environment.NewLine);
    }

    /// <summary>
    /// Adds the given details to the email and log file strings
    /// </summary>
    /// <param name="details">Details to add</param>
    /// <param name="layersIn">Number of 'layers' this log is in to add tabs</param>
    /// <returns>Email and log file string logs</returns>
    private (string emailDetails, string logDetails) AddDetails(IEnumerable<ErrorDetail> details, int layersIn = 0)
    {
        var outcome = (emailDetails: string.Empty, logDetails: string.Empty);

        string tabsToAdd = string.Empty;
        string spacesToAdd = string.Empty;

        for (int i = 0; i < layersIn; i++)
        {
            tabsToAdd += "\t";
            spacesToAdd += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        }

        foreach (var detail in details)
        {
            outcome.emailDetails += $"<br /> {spacesToAdd}{detail.Name}({detail.Type}) = {detail.Value}";
            outcome.logDetails += $"{Environment.NewLine}{tabsToAdd}{detail.Name}({detail.Type}) = {detail.Value}";

            if (detail.ErrorDetails?.Count() > 0)
            {
                var (email, log) = AddDetails(detail.ErrorDetails, layersIn + 1);

                outcome.emailDetails += email;
                outcome.logDetails += log;
            }
        }

        return outcome;
    }

    /// <summary>
    /// Create the error log file
    /// and add the new log content
    /// </summary>
    /// <param name="messageBody">Message Body</param>
    private void CreateErrorLog(string messageBody)
    {
        bool hasAddedLog = false;
        int tryAddLogCount = 0;

        // Try and add the log up to 10 times after 10 goes give up and throw the error
        while (!hasAddedLog && tryAddLogCount < 10)
        {
            try
            {
                tryAddLogCount++;

                string fileName = string.IsNullOrWhiteSpace(_errorSettings.Environment) ? $"{_errorSettings.ApplicationName} Error Log" : $"[{_errorSettings.Environment}] {_errorSettings.ApplicationName} Error Log";

                string fullFilePath = _errorSettings.LogFileDirectory;

                if (_errorSettings.LogFileDirectory[_errorSettings.LogFileDirectory.Length - 1] is not '\\')
                    fullFilePath += @"\";

                fullFilePath += $"{fileName}.txt";

                if (!Directory.Exists(_errorSettings.LogFileDirectory))
                    Directory.CreateDirectory(_errorSettings.LogFileDirectory);

                File.AppendAllText(fullFilePath, Environment.NewLine + messageBody);

                hasAddedLog = true;
            }
            catch
            {
                if (tryAddLogCount >= 10)
                    throw;

                Thread.Sleep(100);
            }
        }
    }

    /// <summary>
    /// Send an e-mail using the given message body and subject. 
    /// Sending the e-mails to each person in the e-mail list
    /// </summary>
    /// <param name="messageBody">E-mail Message body</param>
    /// <param name="subject">E-mail Subject</param>
    /// <param name="emailToList">E-mail To</param>
    /// <param name="emailFrom">E-mail From</param>
    /// <param name="isHtml">is the e-mail HTML</param>
    private void SendEmail(string messageBody, string subject, 
        IEnumerable<string> emailToList, string emailFrom, bool isHtml)
    {
        if (emailToList is not null)
        {
            var message = new MailMessage
            {
                IsBodyHtml = isHtml
            };

            // Add each e-mail to send to
            foreach (string email in emailToList)
                message.To.Add(email);

            message.Subject = subject;
            message.From = new MailAddress(emailFrom);
            message.Body = messageBody;

            var hasSent = false;
            var sendCount = 0;
            while (!hasSent)
            {
                try
                {
                    _smtpClient.Send(message);
                    hasSent = true;
                }
                catch
                {
                    sendCount++;
                    if (sendCount >= 3)
                        throw;
                    else
                        Thread.Sleep(100);
                }
            }
        }
    }
}