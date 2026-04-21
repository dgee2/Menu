using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit3;
using System.Collections;

namespace MenuApi.Integration.Tests.Factory;

/// <summary>
/// AutoData attribute that generates strings short enough to fit in database columns.
/// Default AutoFixture generates strings like "propertyNameGUID" which can exceed varchar(50).
/// This limits generated strings to 20 characters max.
/// It also prevents AutoFixture from auto-populating collection properties on DTOs,
/// since random ingredient/unit names would not exist in the database.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ShortStringAutoDataAttribute : AutoDataAttribute
{
    public ShortStringAutoDataAttribute() : base(() =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new ShortStringSpecimenBuilder());
        fixture.Customizations.Add(new EmptyCollectionPropertyBuilder());
        return fixture;
    })
    {
    }
}

public class ShortStringSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && type == typeof(string))
        {
            return Guid.NewGuid().ToString("N")[..20];
        }

        if (request is System.Reflection.ParameterInfo paramInfo && paramInfo.ParameterType == typeof(string))
        {
            // Use first 8 chars of param name + 12 chars of GUID = 20 chars max
            var prefix = paramInfo.Name?.Length > 8 ? paramInfo.Name[..8] : paramInfo.Name ?? "s";
            return prefix + Guid.NewGuid().ToString("N")[..12];
        }

        return new NoSpecimen();
    }
}

/// <summary>
/// Ensures collection properties on auto-generated DTOs are created as empty
/// rather than being populated with random items (which would reference
/// non-existent database entities).
/// </summary>
public class EmptyCollectionPropertyBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is System.Reflection.PropertyInfo propertyInfo
            && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType)
            && propertyInfo.PropertyType != typeof(string))
        {
            // Return an empty instance of the collection type
            if (propertyInfo.PropertyType.IsGenericType)
            {
                var genericTypeDef = propertyInfo.PropertyType.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(List<>))
                {
                    return Activator.CreateInstance(propertyInfo.PropertyType)!;
                }
            }
        }

        return new NoSpecimen();
    }
}




