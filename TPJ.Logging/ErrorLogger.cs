using System.Net;
using System.Reflection;
using System.Text;
using TPJ.Logging.Models;
using static TPJ.Logging.Enums;

namespace TPJ.Logging;

public interface IErrorLogger
{
	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	void Log(MethodBase methodDetails, Exception exception);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="note">Commonly used to add a meaningful description to the error</param>
	void Log(MethodBase methodDetails, Exception exception, string note);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	void Log<T>(MethodBase methodDetails, Exception exception, T details);

	/// <summary>
	/// Log an error getting details from the method base and the exception
	/// </summary>
	/// <typeparam name="T">Type of the details</typeparam>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	void Log<T>(MethodBase methodDetails, Exception exception, T[] details);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	/// <param name="note">Commonly used to add a meaningful description to the error</param>
	void Log<T>(MethodBase methodDetails, Exception exception, T details, string note);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	Task LogAsync(MethodBase methodDetails, Exception exception);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="note">Add a meaningful description to the error</param>
	Task LogAsync(MethodBase methodDetails, Exception exception, string note);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	Task LogAsync<T>(MethodBase methodDetails, Exception exception, T details);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <typeparam name="T">Type of the details</typeparam>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	Task LogAsync<T>(MethodBase methodDetails, Exception exception, T[] details);

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	/// <param name="note">Add a meaningful description to the error</param>
	Task LogAsync<T>(MethodBase methodDetails, Exception exception, T details, string note);
}

/// <inheritdoc />
public class ErrorLogger : IErrorLogger
{
	private const int DefaultRetryDelayMs = 100;
	private const int DefaultMaxRetryAttempts = 10;

	private readonly IErrorLogSettings _errorSettings;
	private readonly TPJ.Email.IEmailer _emailer;
	private readonly Lock _directoryLock = new();

	public ErrorLogger(IErrorLogSettings errorSettings,
		TPJ.Email.IEmailer emailer)
	{
		_errorSettings = errorSettings ?? throw new ArgumentNullException(nameof(errorSettings));
		_emailer = emailer ?? throw new ArgumentNullException(nameof(emailer));

		ValidateSettings();
	}

	#region Error Log

	#region Sync

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	public void Log(MethodBase methodDetails, Exception exception)
    {
        LogError(methodDetails, exception);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="note">Commonly used to add a meaningful description to the error</param>
	public void Log(MethodBase methodDetails, Exception exception, string note)
    {
        LogError(methodDetails, exception, null, note);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	public void Log<T>(MethodBase methodDetails, Exception exception, T details)
	{
		var objectDetails = BuildErrorDetails(details);
        LogError(methodDetails, exception, objectDetails);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	public void Log<T>(MethodBase methodDetails, Exception exception, T[] details)
	{
		var objectDetails = BuildErrorDetailsArray(details);
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
	public void Log<T>(MethodBase methodDetails, Exception exception, T details, string note)
	{
		var objectDetails = BuildErrorDetails(details);
        LogError(methodDetails, exception, objectDetails, note);
    }

	#endregion

	#region Async

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	public async Task LogAsync(MethodBase methodDetails, Exception exception)
    {
        await LogErrorAsync(methodDetails, exception).ConfigureAwait(false);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="note">Commonly used to add a meaningful description to the error</param>
	public async Task LogAsync(MethodBase methodDetails, Exception exception, string note)
    {
        await LogErrorAsync(methodDetails, exception, null, note).ConfigureAwait(false);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	public async Task LogAsync<T>(MethodBase methodDetails, Exception exception, T details)
	{
		var objectDetails = BuildErrorDetails(details);
        await LogErrorAsync(methodDetails, exception, objectDetails).ConfigureAwait(false);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	public async Task LogAsync<T>(MethodBase methodDetails, Exception exception, T[] details)
	{
		var objectDetails = BuildErrorDetailsArray(details);
        await LogErrorAsync(methodDetails, exception, objectDetails).ConfigureAwait(false);
    }

	/// <summary>
	/// Log an error getting details from 
	/// the method base and the exception
	/// </summary>
	/// <param name="methodDetails">Method Base - System.Reflection.MethodBase.GetCurrentMethod()</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">Takes an object and reads all properties and its details to create a error detail model</param>
	/// <param name="note">Commonly used to add a meaningful description to the error</param>
	public async Task LogAsync<T>(MethodBase methodDetails, Exception exception, T details, string note)
	{
		var objectDetails = BuildErrorDetails(details);
        await LogErrorAsync(methodDetails, exception, objectDetails, note).ConfigureAwait(false);
    }

	#endregion

	#endregion

	#region Private

	/// <summary>
	/// Validate the error log settings configuration
	/// </summary>
	private void ValidateSettings()
	{
		if (_errorSettings.LogType == ErrorLogTypes.LogFile || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			if (string.IsNullOrWhiteSpace(_errorSettings.LogFileDirectory))
				throw new InvalidOperationException("LogFileDirectory is required when LogType includes LogFile");
		}

		if (_errorSettings.LogType == ErrorLogTypes.Email || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			if (_errorSettings.EmailTo == null || !_errorSettings.EmailTo.Any())
				throw new InvalidOperationException("EmailTo is required when LogType includes Email");
		}
	}

	/// <summary>
	/// Build error details from a single object
	/// </summary>
	private static List<ErrorDetailItem> BuildErrorDetails<T>(T details)
	{
		var objectDetails = new List<ErrorDetailItem>();

		if (details != null)
		{
			var (name, errorDetails) = ObjectDetails.Get(details);
			objectDetails.Add(new ErrorDetailItem()
			{
				Name = name,
				ErrorDetails = errorDetails
            });
		}

		return objectDetails;
	}

	/// <summary>
	/// Build error details from an array of objects
	/// </summary>
	private static List<ErrorDetailItem> BuildErrorDetailsArray<T>(T[] details)
	{
		var objectDetails = new List<ErrorDetailItem>();

		if (details != null)
		{
			foreach (var item in details)
			{
				if (item != null)
				{
					var (name, errorDetails) = ObjectDetails.Get(item);
					objectDetails.Add(new ErrorDetailItem()
                    {
                        Name = name,
                        ErrorDetails = errorDetails
                    });
				}
			}
		}

		return objectDetails;
	}

	/// <summary>
	/// Log the error using the passed in 
	/// method details, error exception and user details
	/// </summary>
	/// <param name="methodDetails">Method Details</param>
	/// <param name="exception">Exception</param>
	/// <param name="details">User Details</param>
	/// <param name="note">Add a meaningful description to the error</param>
	private void LogError(MethodBase methodDetails, Exception exception,
		List<ErrorDetailItem>? details = null, string? note = null)
	{
		var (emailBody, logBody) = BuildErrorMessages(methodDetails, exception, details, note);

		if (_errorSettings.LogType == ErrorLogTypes.Email || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			SendEmail(emailBody);
		}

		if (_errorSettings.LogType == ErrorLogTypes.LogFile || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			CreateErrorLog(logBody);
		}
	}

	/// <summary>
	/// Log the error asynchronously
	/// </summary>
	private async Task LogErrorAsync(MethodBase methodDetails, Exception exception,
		List<ErrorDetailItem>? details = null, string? note = null)
	{
		var (emailBody, logBody) = BuildErrorMessages(methodDetails, exception, details, note);

		if (_errorSettings.LogType == ErrorLogTypes.Email || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			await SendEmailAsync(emailBody).ConfigureAwait(false);
		}

		if (_errorSettings.LogType == ErrorLogTypes.LogFile || _errorSettings.LogType == ErrorLogTypes.EmailLogFile)
		{
			await CreateErrorLogAsync(logBody).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Build error message bodies for email and log file
	/// </summary>
	private (string emailBody, string logBody) BuildErrorMessages(MethodBase methodDetails, Exception exception,
		List<ErrorDetailItem>? details, string? note)
	{
		var emailBuilder = new StringBuilder();
		var logBuilder = new StringBuilder();

		// Header information
		emailBuilder.AppendLine($"<b>Date / Time</b>: {DateTime.Now}<br />");
		emailBuilder.AppendLine($"<b>Namespace</b>: {WebUtility.HtmlEncode(methodDetails?.DeclaringType?.FullName)}<br />");
		emailBuilder.AppendLine($"<b>Method Name</b>: {WebUtility.HtmlEncode(methodDetails?.Name)}<br />");
		emailBuilder.AppendLine($"<b>Exception Type</b>: {WebUtility.HtmlEncode(exception.GetType()?.Name)}<br />");
		emailBuilder.AppendLine($"<b>Machine Name</b>: {WebUtility.HtmlEncode(Environment.MachineName)}<br />");
		emailBuilder.AppendLine($"<b>Thread ID</b>: {Environment.CurrentManagedThreadId}<br /><br />");

		logBuilder.AppendLine($"Date / Time: {DateTime.Now}");
		logBuilder.AppendLine($"Namespace: {methodDetails?.DeclaringType?.FullName}");
		logBuilder.AppendLine($"Method name: {methodDetails?.Name}");
		logBuilder.AppendLine($"Exception Type: {exception.GetType()?.Name}");
		logBuilder.AppendLine($"Machine Name: {Environment.MachineName}");
		logBuilder.AppendLine($"Thread ID: {Environment.CurrentManagedThreadId}");

		// Note
		if (!string.IsNullOrWhiteSpace(note))
		{
			emailBuilder.AppendLine($"<b>Note</b>: {WebUtility.HtmlEncode(note)}<br />");
			logBuilder.AppendLine($"Note: {note}");
		}

		// Exception details (including full chain)
		AppendExceptionChain(emailBuilder, logBuilder, exception);

		// Details
		if (details?.Count > 0)
		{
			emailBuilder.AppendLine("<br /><b>Details</b><br />");
			logBuilder.AppendLine("Details");

			foreach (var item in details)
			{
				var (email, log) = AddDetails(item.ErrorDetails);

				emailBuilder.AppendLine($"<b>{WebUtility.HtmlEncode(item.Name)}</b>");
				emailBuilder.Append(email);

				logBuilder.AppendLine(item.Name);
				logBuilder.Append(log);
			}
		}

		var emailTemplate = EmailTemplate.Template()
			.Replace("@Model.Title", GetApplicationTitle())
			.Replace("@Model.SecondHeader", "Error")
			.Replace("@Model.Body", emailBuilder.ToString());

		return (emailTemplate, logBuilder.ToString());
	}

    /// <summary>
    /// Append the full exception chain to email and log builders
    /// </summary>
    private static void AppendExceptionChain(StringBuilder emailBuilder, StringBuilder logBuilder, Exception exception)
	{
		var currentException = exception;
		int level = 0;

		while (currentException != null)
		{
			if (level == 0)
			{
				// Primary exception
				emailBuilder.AppendLine($"<b>Error Message</b>: {WebUtility.HtmlEncode(currentException.Message)}<br />");
				logBuilder.AppendLine($"Error Message: {currentException.Message}");
			}
			else
			{
				// Inner exceptions
				emailBuilder.AppendLine($"<br /><b>Inner Exception (Level {level})</b>: {WebUtility.HtmlEncode(currentException.Message)}<br />");
				logBuilder.AppendLine($"Inner Exception (Level {level}): {currentException.Message}");
			}

			// Always log stack trace if available
			if (!string.IsNullOrWhiteSpace(currentException.StackTrace))
			{
				emailBuilder.AppendLine($"<b>Stack Trace (Level {level})</b>:<br /><pre>{WebUtility.HtmlEncode(currentException.StackTrace)}</pre><br />");
				logBuilder.AppendLine($"Stack Trace (Level {level}): {currentException.StackTrace}");
			}

			currentException = currentException.InnerException;
			level++;
		}
	}

	/// <summary>
	/// Get the application title for email subject
	/// </summary>
	private string GetApplicationTitle()
	{
		return string.IsNullOrWhiteSpace(_errorSettings.Environment)
			? _errorSettings.ApplicationName
			: $"[{_errorSettings.Environment}] {_errorSettings.ApplicationName}";
	}

    /// <summary>
    /// Add details recursively to email and log messages
    /// </summary>
    private static (string emailDetails, string logDetails) AddDetails(IEnumerable<ErrorDetail>? details, int layersIn = 0)
	{

		var emailBuilder = new StringBuilder();
		var logBuilder = new StringBuilder();

		string tabs = new('\t', layersIn);
		string spaces = string.Concat(Enumerable.Repeat("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;", layersIn));

		if (details is not null)
		{
			foreach (var detail in details)
			{
				emailBuilder.AppendLine($"<br />{spaces}{WebUtility.HtmlEncode(detail.Name)}({WebUtility.HtmlEncode(detail.Type)}) = {WebUtility.HtmlEncode(detail.Value)}");
				logBuilder.AppendLine($"{tabs}{detail.Name}({detail.Type}) = {detail.Value}");

				if (detail.ErrorDetails?.Count() > 0)
				{
					var (email, log) = AddDetails(detail.ErrorDetails, layersIn + 1);
					emailBuilder.Append(email);
					logBuilder.Append(log);
				}
			}
		}

		return (emailBuilder.ToString(), logBuilder.ToString());
	}

	/// <summary>
	/// Create the error log file synchronously
	/// </summary>
	private void CreateErrorLog(string messageBody)
	{
		RetryOperation(() =>
		{
			var fullFilePath = GetLogFilePath();
			EnsureDirectoryExists(_errorSettings.LogFileDirectory!);
			File.AppendAllText(fullFilePath, Environment.NewLine + messageBody + Environment.NewLine);
		});
	}

	/// <summary>
	/// Create the error log file asynchronously
	/// </summary>
	private async Task CreateErrorLogAsync(string messageBody)
	{
		await RetryOperationAsync(async () =>
		{
			var fullFilePath = GetLogFilePath();
			EnsureDirectoryExists(_errorSettings.LogFileDirectory!);
			await AppendAllTextAsync(fullFilePath, Environment.NewLine + messageBody + Environment.NewLine).ConfigureAwait(false);
		}).ConfigureAwait(false);
	}

	/// <summary>
	/// Get the full log file path
	/// </summary>
	private string GetLogFilePath()
	{
		string fileName = string.IsNullOrWhiteSpace(_errorSettings.Environment)
			? $"{_errorSettings.ApplicationName} Error Log"
			: $"[{_errorSettings.Environment}] {_errorSettings.ApplicationName} Error Log";

		string directory = _errorSettings.LogFileDirectory!;
		if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
			directory += Path.DirectorySeparatorChar;

		return $"{directory}{fileName}.txt";
	}

	/// <summary>
	/// Ensure the directory exists (thread-safe)
	/// </summary>
	private void EnsureDirectoryExists(string directory)
	{
		if (!Directory.Exists(directory))
		{
			lock (_directoryLock)
			{
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);
			}
		}
	}

    /// <summary>
    /// Retry an operation up to the specified maximum attempts
    /// </summary>
    private static void RetryOperation(Action action, int maxAttempts = DefaultMaxRetryAttempts)
	{
		Exception? lastException = null;

		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				action();
				return;
			}
			catch (Exception ex)
			{
				lastException = ex;
				if (attempt < maxAttempts - 1)
					System.Threading.Thread.Sleep(DefaultRetryDelayMs);
			}
		}

		throw lastException ?? new Exception("Retry operation failed");
	}

    /// <summary>
    /// Retry an async operation up to the specified maximum attempts
    /// </summary>
    private static async Task RetryOperationAsync(Func<Task> action, int maxAttempts = DefaultMaxRetryAttempts)
	{
		Exception? lastException = null;

		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			try
			{
				await action().ConfigureAwait(false);
				return;
			}
			catch (Exception ex)
			{
				lastException = ex;
				if (attempt < maxAttempts - 1)
					await Task.Delay(DefaultRetryDelayMs).ConfigureAwait(false);
			}
		}

		throw lastException ?? new Exception("Retry operation failed");
	}

    /// <summary>
    /// Append text to file asynchronously (.NET Standard 2.0 compatible)
    /// </summary>
    private static async Task AppendAllTextAsync(string path, string contents)
	{
		byte[] encodedText = Encoding.UTF8.GetBytes(contents);

		using var sourceStream = new FileStream(path,
			FileMode.Append, FileAccess.Write, FileShare.None,
			bufferSize: 4096, useAsync: true);
		await sourceStream.WriteAsync(encodedText).ConfigureAwait(false);
	}

	/// <summary>
	/// Send an e-mail synchronously
	/// </summary>
	private void SendEmail(string emailBody)
	{
		if (_errorSettings.EmailTo == null || !_errorSettings.EmailTo.Any())
			return;

		_emailer.SendAsync(CreateEmailMessage(emailBody)).GetAwaiter().GetResult();
	}

	/// <summary>
	/// Send an e-mail asynchronously
	/// </summary>
	private async Task SendEmailAsync(string emailBody)
	{
		if (_errorSettings.EmailTo == null || !_errorSettings.EmailTo.Any())
			return;

		await _emailer.SendAsync(CreateEmailMessage(emailBody)).ConfigureAwait(false);
	}

	/// <summary>
	/// Create email message object
	/// </summary>
	private Email.CreateEmailSingle CreateEmailMessage(string emailBody)
	{
		return new Email.CreateEmailSingle
		{
			Subject = $"{_errorSettings.ApplicationName} Error",
			Body = emailBody,
			To = _errorSettings.EmailTo.Select(x => new Email.CreateEmailAudience { Email = x }),
			From = _errorSettings.EmailFrom == null ? null : new Email.CreateEmailAudience { Email = _errorSettings.EmailFrom }
		};
	}

	#endregion
}