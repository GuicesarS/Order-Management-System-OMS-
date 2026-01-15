namespace OrderManagement.API.Configurations.CustomMapping;

public interface ICustomMapper
{
    TDestination Map<TSource, TDestination>(TSource source);
}
