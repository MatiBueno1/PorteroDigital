using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PorteroDigital.Domain.Entities;

namespace PorteroDigital.Infrastructure.Security;

public sealed class ResidentTokenService(IOptions<JwtOptions> options)
{
    public (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateToken(Resident resident, House house)
    {
        var jwtOptions = options.Value;
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, resident.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, resident.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, resident.Email),
            new Claim("house_id", house.Id.ToString()),
            new Claim("resident_name", resident.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
