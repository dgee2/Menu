using AutoFixture.Kernel;
using System.CodeDom.Compiler;
using Vogen;

namespace MenuApi.Tests;

internal class ValueObjectSpecimenBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is Type type && IsValueObject(type))
        {
            var value = context.Resolve(GetUnderlyingType(type));

            return type.GetMethod("From").Invoke(null, [value]);
        }

        return new NoSpecimen();
    }

    public static bool IsValueObject(Type type)
    {
        return Attribute.IsDefined(type, typeof(ValueObjectAttribute));
    }

    public static Type GetUnderlyingType(Type type)
    {
        var isVogen = type.GetCustomAttributes(typeof(GeneratedCodeAttribute), false)
                                 .OfType<GeneratedCodeAttribute>()
                                 .Any(x=>x.Tool == "Vogen");

        if (!isVogen)
        {
            throw new InvalidOperationException($"Type {type.Name} is not a Vogen value object.");
        }

        var propertyType = type.GetProperty("Value")?.PropertyType;

        if(propertyType == null)
        {
            throw new InvalidOperationException($"Type {type.Name} does not have a Value property.");
        }
        return propertyType;

    }
}
