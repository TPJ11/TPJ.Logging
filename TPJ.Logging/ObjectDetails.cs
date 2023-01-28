using System.Collections;
using System.Reflection;

namespace TPJ.Logging;

internal static class ObjectDetails
{
    /// <summary>
    /// Gets all the properties for a class then loops over them 
    /// getting the name, type and value E.G FirstName, String, Thomas
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    internal static (string name, List<ErrorDetail> errorDetails) Get<T>(T details)
    {
        var objectDetails = new List<ErrorDetail>();
        var objectName = string.Empty;

        try
        {
            var objectType = details.GetType();

            objectName = objectType.FullName;

            var properties = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (properties.Length == 0)
            {
                objectDetails.Add(new ErrorDetail("", details.ToString(), objectName));
            }
            else
            {
                foreach (var objectPropertyInfo in properties)
                {
                    if (objectPropertyInfo.CanRead)
                    {
                        var value = objectPropertyInfo.GetValue(details, null);

                        #region Get Type E.G Int32 or String or class name
                        var propertyType = objectPropertyInfo.PropertyType;

                        if (propertyType.IsGenericType
                            && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            propertyType = Nullable.GetUnderlyingType(propertyType);
                        }

                        #endregion

                        if (!propertyType.IsGenericType
                            && propertyType.IsClass
                            && value != null
                            && !IsCSharpType(propertyType)
                            && propertyType?.BaseType?.FullName != "System.Array")
                        {
                            objectDetails.Add(new ErrorDetail(objectPropertyInfo.Name, null, propertyType.Name, Get(value).errorDetails));
                        }
                        else if (IsCSharpType(propertyType) || value is null)
                        {
                            if (IsSensitive(objectPropertyInfo))
                                objectDetails.Add(new ErrorDetail(objectPropertyInfo.Name, "##Redacted##", propertyType.Name));
                            else
                                objectDetails.Add(new ErrorDetail(objectPropertyInfo.Name, value?.ToString(), propertyType.Name));
                        }
                        else if (IsList(propertyType))
                        {
                            var list = (IEnumerable)value;

                            foreach (var item in list)
                                objectDetails.Add(new ErrorDetail(objectPropertyInfo.Name, null, propertyType.Name, Get(item).errorDetails));
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return (objectName, objectDetails);
    }

    /// <summary>
    /// Contains the list of all interface list types
    /// </summary>
    private static readonly List<Type> _listTypes = new() { typeof(IList), typeof(ICollection), typeof(IEnumerable) };

    /// <summary>
    /// Checks to see if the given type is a list type
    /// </summary>
    /// <param name="t">Type to check if it is a list</param>
    /// <returns>[True] the type is a list else [False] type is not a list</returns>
    private static bool IsList(Type t) => t.GetInterfaces().Any(i => _listTypes.Contains(i));

    /// <summary>
    /// Checks to see if the given property info contains 'Sensitive' data attribute
    /// </summary>
    /// <param name="objectPropertyInfo">property to check</param>
    /// <returns>[True] have 'Sensitive' data attribute else [False] does not have 'Sensitive' data attribute</returns>
    private static bool IsSensitive(PropertyInfo objectPropertyInfo)
    {
        var attrs = objectPropertyInfo.GetCustomAttributes(true);
        foreach (var attr in attrs)
        {
            if (attr is Sensitive)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks to see if the type is a "C#" type e.g a string
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>If [True] is C# type else [False]</returns>
    private static bool IsCSharpType(Type type) =>
        type == typeof(bool)
        || type == typeof(byte)
        || type == typeof(char)
        || type == typeof(decimal)
        || type == typeof(double)
        || type == typeof(float)
        || type == typeof(int)
        || type == typeof(long)
        || type == typeof(sbyte)
        || type == typeof(short)
        || type == typeof(uint)
        || type == typeof(ulong)
        || type == typeof(ushort)
        || type == typeof(string);
}