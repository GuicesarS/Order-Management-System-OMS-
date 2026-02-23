using AutoMapper;

namespace OrderManagement.Application.Common.CustomMapping;

public class CustomMapper : ICustomMapper
{
    private readonly IMapper _mapper;

    public CustomMapper(IMapper mapper)
    {
        _mapper = mapper;
    }
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        return _mapper.Map<TDestination>(source);
    }
}
