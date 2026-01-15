using ExpressMapper;

namespace OrderManagement.API.Configurations.CustomMapping;

public class CustomMapper : ICustomMapper
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return Mapper.Map<TSource, TDestination>(source);
    }
}
