using OrderManagement.Domain.Entities.User;

namespace OrderManagement.Domain.Security.Token;

public interface IAccessTokenGenerator
{
    string Generate(User user);
}
