using AutoMapper;

namespace UnitTests.Utils
{
    internal static class MapperUtils
    {
        public static IMapper CreateMapper<T>() where T : Profile, new()
        {
            var mappingProfile = new T();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mappingProfile));
            return configuration.CreateMapper();
        }
    }
}
