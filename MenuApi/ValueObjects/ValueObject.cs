using Dapper;
using System.Collections.Immutable;
using System.Reflection;

namespace MenuApi.ValueObjects;

public static class ValueObject
{
    public static void ConfigureDapperTypeHandlers()
    {
        ImmutableArray<Type> typeHandlers = GetDapperTypeHandlers();

        foreach (var typeHandler in typeHandlers)
        {
            RegisterDapperTypeHandler(typeHandler);
        }
    }

    private static void RegisterDapperTypeHandler(Type typeHandler)
    {
        var valueObjectType = typeHandler.BaseType?.GenericTypeArguments[0];
        if (valueObjectType == null) return;

        var instance = (SqlMapper.ITypeHandler?)typeHandler.GetConstructor(Type.EmptyTypes)?.Invoke([]);
        if (instance == null) return;

        SqlMapper.AddTypeHandler(valueObjectType, instance);
    }

    private static ImmutableArray<Type> GetDapperTypeHandlers()
    {
        var interfaceType = typeof(SqlMapper.ITypeHandler);

        var typeHandlers = Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Contains(interfaceType))
            .ToImmutableArray();
        return typeHandlers;
    }
}
