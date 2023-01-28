namespace TPJ.LoggingTest.Models;

class Scope
{
    private string Private { get; set; }
    public string PrivateGet { private get; set; }
    public string PrivateSet { get; private set; }

    internal string Internal { get; set; }
    public string InternalGet { internal get; set; }
    public string InternalSet { get; internal set; }

    public Scope()
    {
        Private = "A";
        PrivateGet = "B";
        PrivateSet = "C";
        Internal = "D";
        InternalGet = "E";
        InternalSet = "F";
    }
}
