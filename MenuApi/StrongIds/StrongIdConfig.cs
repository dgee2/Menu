using Dapper;
using System.Collections.Immutable;
using System.Reflection;

namespace MenuApi.StrongIds;

public static class StrongIdConfig
{
    public static void ConfigureStrongIds()
    {
        var interfaceType = typeof(SqlMapper.ITypeHandler);

        var typeHandlers = Assembly.GetCallingAssembly().GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Contains(interfaceType))
            .ToImmutableArray();

        foreach (var typeHandler in typeHandlers)
        {
            var strongIdType = typeHandler.BaseType?.GenericTypeArguments[0];
            if (strongIdType == null) continue;

            var instance = (SqlMapper.ITypeHandler?)typeHandler.GetConstructor(Type.EmptyTypes)?.Invoke([]);
            if (instance == null) continue;

            SqlMapper.AddTypeHandler(strongIdType, instance);
        }
    }
}
