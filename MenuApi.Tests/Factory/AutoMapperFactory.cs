using AutoMapper;

namespace MenuApi.Tests.Factory;

static class AutoMapperFactory
{
    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Program).Assembly));
        return new Mapper(configuration);
    }
}
