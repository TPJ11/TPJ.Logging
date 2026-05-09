namespace TPJ.Logging.Models;

internal class ErrorDetailItem
{
    public string? Name { get; set; }
    public IEnumerable<ErrorDetail>? ErrorDetails { get; set; }
}

internal class ErrorDetail
{
    internal string? Name { get; set; }
    internal string? Value { get; set; }
    internal string? Type { get; set; }

    internal IEnumerable<ErrorDetail>? ErrorDetails { get; set; }
}
