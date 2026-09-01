using System.Reflection;
using AutoMapper;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        bool ImplementsMapFrom(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(ImplementsMapFrom));

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName)
                ?? type.GetInterfaces().First(ImplementsMapFrom).GetMethod(mappingMethodName);

            methodInfo?.Invoke(instance, [this]);
        }
    }
}
