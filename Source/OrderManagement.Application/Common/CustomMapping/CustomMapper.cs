using ExpressMapper;

namespace OrderManagement.Application.Common.CustomMapping;

public class CustomMapper : ICustomMapper
{
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return Mapper.Map<TSource, TDestination>(source);
    }
}
