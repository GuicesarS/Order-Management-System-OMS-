namespace OrderManagement.Domain.Security.Token;

public interface IAccessTokenGenerator
{
    string Generate(Guid userId);
}
