# TPJ.Logging
Simple error logging library that can send emails and/or log to a txt file

## Setup

### appsettings.json
`ApplicationName` – (Required) Name of the application used on the log file names and e-mails 

`ErrorLogType` (Required) - There are three types of error log types:
1) Email - Errors are sent via e-mail only (as per rest of the config settings) 
2) LogFile - Errors are logged in a txt file (named – `[{Environment}] {Application Name} Error Log.txt`) 
3) EmailLogFile - Does both Email and LogFile 

`LogFileDirectory` (Required for log file logging) - The location at which the log / error file will be placed 

`To` (Required for e-mail logging) - Error e-mails sent to; Can be a list split by `;` E.G "Test@test.com;Test2@test.com"

`From` (Required for e-mail logging) - E-mails are sent from this account 

`SmtpClient` (Required for e-mail logging) - SMTP server which e-mails will be sent from 

`SmtpUser` - Send e-mail using the given user name and password 

`SmtpPassword` - Send e-mail using the given user name and password 

`Port` - Port to send from 

`EnableSSL` - Enable SSL when sending the e-mail

#### Example
Within  `appsettings.json` and add (add your own settings):
```
"TPJ": {
  "Logging": {
    "ApplicationName": "Test App",
    "LogType": "EmailLogFile",
    "LogFileDirectory": "C:\\Test",
    "Email": {
      "From": "test-app@test.com",
      "SmtpClient": "smtp.gmail.com",
      "SmtpUser": "abc",
      "SmtpPassword": "xyz",
      "EnableSSL": true,
      "Port": 587
    }
  }
}
```
Within `appsettings.{environment}.json` add:

```
"TPJ": {
  "Logging": {
    "Email": {
      "To": "example@test.com"
    }
  }
}
```

### ConfigureServices
To add logging simply add TPJ logging to your DI:
```
services.AddTPJLogging();
```
## Example Error Log
Check out the git repo code for example setup for a console app and API.

```
private readonly TPJ.Logging.ILogger _logger; 

public HomeController(TPJ.Logging.ILogger logger) 
{ 
    _logger = logger; 
} 
```
Then within an `IActionResult` you might have this 
```
public IActionResult About(Divide divide) 
{ 
	try 
	{ 
		var divideByZero = divide.ValueOne / divide.ValueTwo;
		return View(); 
	} 
	catch (Exception e) 
	{ 
		_logger.Log(System.Reflection.MethodBase.GetCurrentMethod(), e, divide);
		return RedirectToAction(nameof(Error)); 
	} 
}
```
This will then log the error with the current method information, the exception details and the details of the object `divide` – it will log nested classes within objects.

## GDPR Sensitive Data
Any data that is personal / sensitive such as e-mail, address, passwords should not be logged or sent over e-mail. To hide this information you add `[Sensitive]` data attriribute to the property that contains the sensitive information.

```
[Sensitive]
public string Password { get; set; }
```

This will then remove the data when logging and place `##Redacted##` instead.

