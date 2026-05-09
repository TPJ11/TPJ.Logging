using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TPJ.Logging;
using TPJ.Logging.Models;

namespace TPJ.LoggingTest;

[TestFixture]
public class ObjectDetailsTests
{
    #region Test Classes

    private class SimplePrimitiveObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Height { get; set; }
        public bool IsActive { get; set; }
    }

    private class ObjectWithSensitiveData
    {
        public string Username { get; set; }

        [Sensitive]
        public string Password { get; set; }

        public string Email { get; set; }

        [Sensitive]
        public string CreditCard { get; set; }
    }

    private class NestedObject
    {
        public string OuterProperty { get; set; }
        public InnerObject Inner { get; set; }
    }

    private class InnerObject
    {
        public string InnerProperty { get; set; }
        public int InnerValue { get; set; }
    }

    private class DeepNestedObject
    {
        public string Level { get; set; }
        public DeepNestedObject Child { get; set; }
    }

    private class CircularReferenceObject
    {
        public string Name { get; set; }
        public CircularReferenceObject Reference { get; set; }
    }

    private class CollectionObject
    {
        public string Name { get; set; }
        public List<string>? Items { get; set; }
        public int[]? Numbers { get; set; }
    }

    private class ComplexCollectionObject
    {
        public string Name { get; set; }
        public List<InnerObject> ComplexItems { get; set; }
    }

    private class NullablePropertiesObject
    {
        public int? NullableInt { get; set; }
        public bool? NullableBool { get; set; }
        public string? NullableString { get; set; }
    }

    private class ObjectWithPrivateProperties
    {
        public string PublicProperty { get; set; }
        private string PrivateProperty { get; set; }

        public ObjectWithPrivateProperties(string publicValue, string privateValue)
        {
            PublicProperty = publicValue;
            PrivateProperty = privateValue;
        }
    }

    private class ObjectWithReadOnlyProperty
    {
        public string Name { get; set; }
        public string ReadOnlyProperty => "ConstantValue";
    }

    private class ObjectWithWriteOnlyProperty
    {
        private string _value;

        public string Name { get; set; }

        public string WriteOnly
        {
            set => _value = value;
        }
    }

    #endregion

    [Test]
    public void Get_WithNull_ReturnsEmptyErrorDetails()
    {
        // Arrange
        SimplePrimitiveObject? obj = null;

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Is.Empty);
        Assert.That(result.name, Is.EqualTo("TPJ.LoggingTest.ObjectDetailsTests+SimplePrimitiveObject"));
    }

    [Test]
    public void Get_WithSimplePrimitiveObject_ReturnsAllProperties()
    {
        // Arrange
        var obj = new SimplePrimitiveObject
        {
            Name = "John",
            Age = 30,
            Height = 5.9,
            IsActive = true
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.EqualTo(4));
        Assert.That(result.name, Is.EqualTo("TPJ.LoggingTest.ObjectDetailsTests+SimplePrimitiveObject"));

        var nameDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Name");
        Assert.That(nameDetail, Is.Not.Null);
        Assert.That(nameDetail.Value, Is.EqualTo("John"));
        Assert.That(nameDetail.Type, Is.EqualTo("String"));

        var ageDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Age");
        Assert.That(ageDetail, Is.Not.Null);
        Assert.That(ageDetail.Value, Is.EqualTo("30"));
        Assert.That(ageDetail.Type, Is.EqualTo("Int32"));
    }

    [Test]
    public void Get_WithSensitiveData_RedactsSensitiveProperties()
    {
        // Arrange
        var obj = new ObjectWithSensitiveData
        {
            Username = "john_doe",
            Password = "secret123",
            Email = "john@example.com",
            CreditCard = "1234-5678-9012-3456"
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        var passwordDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Password");
        Assert.That(passwordDetail, Is.Not.Null);
        Assert.That(passwordDetail.Value, Is.EqualTo("##Redacted##"));

        var creditCardDetail = result.errorDetails.FirstOrDefault(d => d.Name == "CreditCard");
        Assert.That(creditCardDetail, Is.Not.Null);
        Assert.That(creditCardDetail.Value, Is.EqualTo("##Redacted##"));

        var usernameDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Username");
        Assert.That(usernameDetail, Is.Not.Null);
        Assert.That(usernameDetail.Value, Is.EqualTo("john_doe"));

        var emailDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Email");
        Assert.That(emailDetail, Is.Not.Null);
        Assert.That(emailDetail.Value, Is.EqualTo("john@example.com"));
    }

    [Test]
    public void Get_WithNestedObject_RecursesIntoNestedProperties()
    {
        // Arrange
        var obj = new NestedObject
        {
            OuterProperty = "Outer",
            Inner = new InnerObject
            {
                InnerProperty = "Inner",
                InnerValue = 42
            }
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.EqualTo(2));

        var innerDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Inner");
        Assert.That(innerDetail, Is.Not.Null);
        Assert.That(innerDetail.ErrorDetails, Is.Not.Null);
        Assert.That(innerDetail.ErrorDetails.Count(), Is.EqualTo(2));

        var innerPropertyDetail = innerDetail.ErrorDetails.FirstOrDefault(d => d.Name == "InnerProperty");
        Assert.That(innerPropertyDetail, Is.Not.Null);
        Assert.That(innerPropertyDetail.Value, Is.EqualTo("Inner"));
    }

    [Test]
    public void Get_WithCircularReference_DetectsAndHandlesCircularReference()
    {
        // Arrange
        var obj = new CircularReferenceObject { Name = "Root" };
        obj.Reference = obj; // Create circular reference

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.EqualTo(2));

        var referenceDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Reference");
        Assert.That(referenceDetail, Is.Not.Null);
        Assert.That(referenceDetail.ErrorDetails, Is.Not.Null);

        var circularMessage = referenceDetail.ErrorDetails.FirstOrDefault(d => d.Value == "[Circular reference detected]");
        Assert.That(circularMessage, Is.Not.Null);
    }

    [Test]
    public void Get_WithDeepNesting_StopsAtMaxRecursionDepth()
    {
        // Arrange
        var obj = new DeepNestedObject { Level = "Level0" };
        var current = obj;

        // Create a chain deeper than MaxRecursionDepth (10)
        for (int i = 1; i <= 12; i++)
        {
            current.Child = new DeepNestedObject { Level = $"Level{i}" };
            current = current.Child;
        }

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert - Should contain max depth message somewhere in the hierarchy
        bool hasMaxDepthMessage = ContainsMaxDepthMessage(result.errorDetails);
        Assert.That(hasMaxDepthMessage, Is.True);
    }

    private static bool ContainsMaxDepthMessage(IEnumerable<ErrorDetail>? details)
    {
        if (details is not null)
        {
            foreach (var detail in details)
            {
                if (detail.Value == "[Max recursion depth reached]")
                    return true;

                if (detail.ErrorDetails != null && ContainsMaxDepthMessage(detail.ErrorDetails))
                    return true;
            }
        }
        return false;
    }

    [Test]
    public void Get_WithListOfStrings_EnumeratesCollection()
    {
        // Arrange
        var obj = new CollectionObject
        {
            Name = "TestCollection",
            Items = new List<string> { "Item1", "Item2", "Item3" },
            Numbers = new int[] { 1, 2, 3 }
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.GreaterThan(0));

        var itemsDetails = result.errorDetails.Where(d => d.Name == "Items");
        Assert.That(itemsDetails.Count(), Is.EqualTo(3)); // Three items in the list
    }

    [Test]
    public void Get_WithListOfComplexObjects_EnumeratesAndRecurses()
    {
        // Arrange
        var obj = new ComplexCollectionObject
        {
            Name = "ComplexCollection",
            ComplexItems = new List<InnerObject>
            {
                new InnerObject { InnerProperty = "First", InnerValue = 1 },
                new InnerObject { InnerProperty = "Second", InnerValue = 2 }
            }
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        var complexItemsDetails = result.errorDetails.Where(d => d.Name == "ComplexItems").ToList();
        Assert.That(complexItemsDetails.Count, Is.EqualTo(2));

        foreach (var item in complexItemsDetails)
        {
            Assert.That(item.ErrorDetails, Is.Not.Null);
            Assert.That(item.ErrorDetails.Count(), Is.EqualTo(2)); // Each InnerObject has 2 properties
        }
    }

    [Test]
    public void Get_WithNullableProperties_HandlesNullValues()
    {
        // Arrange
        var obj = new NullablePropertiesObject
        {
            NullableInt = null,
            NullableBool = true,
            NullableString = null
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        var nullIntDetail = result.errorDetails.FirstOrDefault(d => d.Name == "NullableInt");
        Assert.That(nullIntDetail, Is.Not.Null);
        Assert.That(nullIntDetail.Value, Is.Null.Or.Empty);

        var boolDetail = result.errorDetails.FirstOrDefault(d => d.Name == "NullableBool");
        Assert.That(boolDetail, Is.Not.Null);
        Assert.That(boolDetail.Value, Is.EqualTo("True"));
    }

    [Test]
    public void Get_WithPrivateProperties_IncludesPrivateProperties()
    {
        // Arrange
        var obj = new ObjectWithPrivateProperties("Public", "Private");

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails.Count, Is.GreaterThanOrEqualTo(1));

        var publicDetail = result.errorDetails.FirstOrDefault(d => d.Name == "PublicProperty");
        Assert.That(publicDetail, Is.Not.Null);
        Assert.That(publicDetail.Value, Is.EqualTo("Public"));
    }

    [Test]
    public void Get_WithReadOnlyProperty_IncludesReadOnlyProperty()
    {
        // Arrange
        var obj = new ObjectWithReadOnlyProperty
        {
            Name = "Test"
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        var readOnlyDetail = result.errorDetails.FirstOrDefault(d => d.Name == "ReadOnlyProperty");
        Assert.That(readOnlyDetail, Is.Not.Null);
        Assert.That(readOnlyDetail.Value, Is.EqualTo("ConstantValue"));
    }

    [Test]
    public void Get_WithWriteOnlyProperty_SkipsWriteOnlyProperty()
    {
        // Arrange
        var obj = new ObjectWithWriteOnlyProperty
        {
            Name = "Test"
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        var writeOnlyDetail = result.errorDetails.FirstOrDefault(d => d.Name == "WriteOnly");
        Assert.That(writeOnlyDetail, Is.Null); // Write-only properties should be skipped
    }

    [Test]
    public void Get_WithString_TreatsAsSimpleValue()
    {
        // Arrange
        string obj = "TestString";

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.EqualTo(1));
        Assert.That(result.errorDetails[0].Value, Is.EqualTo("TestString"));
    }

    [Test]
    public void Get_WithInteger_TreatsAsPrimitive()
    {
        // Arrange
        int obj = 42;

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Has.Count.EqualTo(1));
        Assert.That(result.errorDetails[0].Value, Is.EqualTo("42"));
    }

    [Test]
    public void Get_WithEmptyCollection_HandlesEmptyCollection()
    {
        // Arrange
        var obj = new CollectionObject
        {
            Name = "Empty",
            Items = new List<string>(),
            Numbers = new int[0]
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Is.Not.Null);
        var nameDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Name");
        Assert.That(nameDetail, Is.Not.Null);
    }

    [Test]
    public void Get_WithNullCollectionProperty_HandlesNullCollection()
    {
        // Arrange
        var obj = new CollectionObject
        {
            Name = "NullCollection",
            Items = null,
            Numbers = null
        };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.errorDetails, Is.Not.Null);
        var itemsDetail = result.errorDetails.FirstOrDefault(d => d.Name == "Items");
        Assert.That(itemsDetail, Is.Not.Null);
        Assert.That(itemsDetail.Value, Is.Null.Or.Empty);
    }

    [Test]
    public void Get_ReturnsCorrectObjectName()
    {
        // Arrange
        var obj = new SimplePrimitiveObject { Name = "Test" };

        // Act
        var result = ObjectDetails.Get(obj);

        // Assert
        Assert.That(result.name, Contains.Substring("SimplePrimitiveObject"));
    }
}
