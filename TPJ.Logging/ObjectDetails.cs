using TPJ.Logging.Models;
using System.Collections;
using System.Reflection;

namespace TPJ.Logging;

internal static class ObjectDetails
{
	private static readonly HashSet<Type> _primitiveTypes =
    [
        typeof(bool),
		typeof(byte),
		typeof(sbyte),
		typeof(char),
		typeof(decimal),
		typeof(double),
		typeof(float),
		typeof(int),
		typeof(uint),
		typeof(long),
		typeof(ulong),
		typeof(short),
		typeof(ushort),
		typeof(string)
	];

	private static readonly HashSet<Type> _collectionInterfaces =
    [
        typeof(IList),
		typeof(ICollection),
		typeof(IEnumerable)
	];

	private const int MaxRecursionDepth = 10;

	/// <summary>
	/// Gets all the properties for a class then loops over them 
	/// getting the name, type and value E.G FirstName, String, Thomas
	/// </summary>
	/// <param name="details">The object to extract details from</param>
	/// <returns>A tuple containing the object's full name and a list of error details</returns>
	internal static (string? name, List<ErrorDetail> errorDetails) Get<T>(T details)
	{
		return GetInternal(details, 0, []);
	}

	private static (string? name, List<ErrorDetail> errorDetails) GetInternal<T>(T details, int depth, HashSet<object> visitedObjects)
	{
		if (details == null)
		{
			return (typeof(T).FullName, new List<ErrorDetail>());
		}

		// Prevent stack overflow from deep recursion
		if (depth >= MaxRecursionDepth)
		{
			return (details.GetType().FullName, new List<ErrorDetail>
			{
				new()
				{
					Name = "",
					Value = "[Max recursion depth reached]",
					Type = details.GetType().Name
                }
			});
		}

		// Prevent circular references (only track reference types)
		if (!details.GetType().IsValueType && visitedObjects.Contains(details))
		{
			return (details.GetType().FullName, new List<ErrorDetail>
			{
				new()
				{
					Name = "",
					Value = "[Circular reference detected]",
					Type = details.GetType().Name
				}
			});
		}

		List<ErrorDetail> objectDetails = [];
		Type objectType = details.GetType();
		var objectName = objectType.FullName;

		// Add to visited set for reference types
		if (!objectType.IsValueType)
		{
			visitedObjects.Add(details);
		}

		var properties = objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

		// Handle primitive types and types with no readable properties
		if (properties.Length == 0 || IsPrimitiveType(objectType))
		{
			objectDetails.Add(new()
			{
				Name = "",
				Value = details.ToString(),
				Type = objectName
            });
        }
		else
		{
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!propertyInfo.CanRead)
				{
					continue;
				}

				try
				{
					var value = propertyInfo.GetValue(details, null);
					var propertyType = GetActualPropertyType(propertyInfo.PropertyType);
					var isSensitive = IsSensitive(propertyInfo);

					if (ShouldRecurseIntoObject(propertyType, value))
					{
						var (name, errorDetails) = GetInternal(value, depth + 1, visitedObjects);
						objectDetails.Add(new()
						{
							Name = propertyInfo.Name,
							Type = propertyType.Name,
							ErrorDetails = errorDetails
                        });
                    }
					else if (IsPrimitiveType(propertyType) || value == null)
					{
						var displayValue = isSensitive ? "##Redacted##" : value?.ToString();
						objectDetails.Add(new()
						{
							Name = propertyInfo.Name,
							Value = displayValue,
							Type = propertyType.Name
                        });
                    }
					else if (IsCollection(propertyType))
					{
						var collection = (IEnumerable)value;
						foreach (var item in collection)
						{
							if (item != null)
							{
								var (name, errorDetails) = GetInternal(item, depth + 1, visitedObjects);
								objectDetails.Add(new()
								{
									Name = propertyInfo.Name,
									Type = propertyType.Name,
									ErrorDetails = errorDetails
                                });
                            }
						}
					}
				}
				catch (Exception)
				{
					// If property access fails, skip it
					objectDetails.Add(new()
					{
						Name = propertyInfo.Name,
						Value = "[Error accessing property]",
						Type = propertyInfo.PropertyType.Name
                    });
                }
			}
		}

		return (objectName, objectDetails);
	}

	/// <summary>
	/// Gets the underlying type, unwrapping Nullable types
	/// </summary>
	private static Type GetActualPropertyType(Type propertyType)
	{
		if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return Nullable.GetUnderlyingType(propertyType)!;
		}

		return propertyType;
	}

	/// <summary>
	/// Determines if the object should be recursed into for detailed inspection
	/// </summary>
	private static bool ShouldRecurseIntoObject(Type propertyType, object? value)
	{
		return !propertyType.IsGenericType
			&& propertyType.IsClass
			&& value != null
			&& !IsPrimitiveType(propertyType)
			&& !IsCollection(propertyType)
			&& propertyType.BaseType?.FullName != "System.Array";
	}

	/// <summary>
	/// Checks if the type is a collection type that should be enumerated
	/// </summary>
	private static bool IsCollection(Type type)
	{
		// String is IEnumerable but shouldn't be treated as a collection
		if (type == typeof(string))
		{
			return false;
		}

		// Check if the type itself is a collection interface
		if (_collectionInterfaces.Contains(type))
		{
			return true;
		}

		// Check if the type implements any collection interfaces
		return type.GetInterfaces().Any(i => _collectionInterfaces.Contains(i));
	}

	/// <summary>
	/// Checks if the type is a primitive C# type (value types and string)
	/// </summary>
	/// <param name="type">Type to check</param>
	/// <returns>True if primitive type, otherwise false</returns>
	private static bool IsPrimitiveType(Type type)
	{
		return _primitiveTypes.Contains(type);
	}

	/// <summary>
	/// Checks if a property is marked as sensitive and should be redacted
	/// </summary>
	/// <param name="propertyInfo">The property to check</param>
	/// <returns>True if property is sensitive, otherwise false</returns>
	private static bool IsSensitive(PropertyInfo propertyInfo)
	{
		return propertyInfo.GetCustomAttribute<Sensitive>() != null;
	}
}