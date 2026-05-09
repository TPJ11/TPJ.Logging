namespace TPJ.Logging;

/// <summary>
/// Makers the property as sensitive so any data held within it will not be logged in the error
/// E.G. Password
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class Sensitive : Attribute
{
}
