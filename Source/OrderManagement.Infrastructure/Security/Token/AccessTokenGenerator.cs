using Microsoft.IdentityModel.Tokens;
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

    public string Generate(Guid userId)
    {
        var claims = new List<Claim>()
        {
            // Aqui estamos colocando apenas o identificador do usuário
            // ClaimTypes.NameIdentifier é o padrão para "id do usuário"
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        // SecurityTokenDescriptor descreve como o token será criado
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Identidade do token (quem é o usuário)
            Subject = new ClaimsIdentity(claims),

            // Data de expiração do token
            // Após esse tempo, o token não é mais válido
            Expires = DateTime.UtcNow.AddMinutes(_expirationToken),

            // Define como o token será assinado
            // Usamos HMAC SHA256 + chave simétrica
            SigningCredentials = new SigningCredentials(
                SecurityKey(),
                SecurityAlgorithms.HmacSha256
            )
        };

        // Responsável por criar e escrever o JWT
        var tokenHandler = new JwtSecurityTokenHandler();

        // Cria o token baseado no descriptor
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        // Serializa o token para string (formato JWT)
        return tokenHandler.WriteToken(securityToken);
    }

    // Cria a chave de segurança usada para assinar o token
    // A string da configuração é convertida para bytes
    private SymmetricSecurityKey SecurityKey()
    {   
        var bytes = Encoding.UTF8.GetBytes(_signingKey);

        return new SymmetricSecurityKey(bytes);
    }
}
