using Microsoft.IdentityModel.Tokens;
using OrderManagement.Domain.Entities.User;
using OrderManagement.Domain.Security.Token;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.Infrastructure.Security.Token;

public class AccessTokenGenerator : IAccessTokenGenerator
{
    private readonly int _expirationToken;
    private readonly string _signingKey;

    public AccessTokenGenerator(int expirationToken, string signingKey)
    {
        _expirationToken = expirationToken;
        _signingKey = signingKey;
    }

    public string Generate(User user)
    {
        var claims = new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),

            Expires = DateTime.UtcNow.AddMinutes(_expirationToken),

            SigningCredentials = new SigningCredentials(
                SecurityKey(),
                SecurityAlgorithms.HmacSha256
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
    private SymmetricSecurityKey SecurityKey()
    {   
        var bytes = Encoding.UTF8.GetBytes(_signingKey);

        return new SymmetricSecurityKey(bytes);
    }
}
