namespace OrderManagement.Application.Common.CustomMapping;

public interface ICustomMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
}
