namespace TPJ.Logging;

internal class ErrorDetailItem
{
    public string Name { get; set; }
    public IEnumerable<ErrorDetail> ErrorDetails { get; set; }

    public ErrorDetailItem()
    {

    }

    public ErrorDetailItem(string name, IEnumerable<ErrorDetail> errorDetails)
    {
        Name = name;
        ErrorDetails = errorDetails;
    }
}

internal class ErrorDetail
{
    internal string Name { get; set; }
    internal string Value { get; set; }
    internal string Type { get; set; }

    internal IEnumerable<ErrorDetail> ErrorDetails { get; set; }

    /// <summary>
    /// Error Details
    /// </summary>
    /// <param name="name">Name of the property E.G. FirstName</param>
    /// <param name="value">Value of the property E.G. Thomas</param>
    /// <param name="type">Type of the property E.G String</param>
    internal ErrorDetail(string name, string value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Error Details
    /// </summary>
    /// <param name="name">Name of the property E.G. FirstName</param>
    /// <param name="value">Value of the property E.G. Thomas</param>
    /// <param name="type">Type of the property E.G String</param>
    /// <param name="errorDetails">List of the above class</param>
    internal ErrorDetail(string name, string value, string type, IEnumerable<ErrorDetail> errorDetails)
    {
        Name = name;
        Value = value;
        Type = type;
        ErrorDetails = errorDetails;
    }
}
