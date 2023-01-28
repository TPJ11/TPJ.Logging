using TPJ.Logging;

namespace TPJ.LoggingTest.Models;

class TestSensitive
{
    public string UserName { get; set; }

    [Sensitive]
    public string Password { get; set; }
}