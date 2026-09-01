using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.UnitTests.Common.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.Load("Application")));
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void Configuration_IsValid()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
