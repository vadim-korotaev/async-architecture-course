using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Common;

public static class TokenService
{
    private const string SECRET_KEY = @"Walt: Are you sure? That's right. Now. Say my name. 
                                      Declan: You're Heisenberg. Walt: You're goddamn right.";
    
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SECRET_KEY));
    }
    public static string BuildToken(string username, Guid userPublicId, UserRole userRole)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("id", userPublicId.ToString()),
            new Claim(ClaimTypes.Role, userRole.ToString())
        };
        
        var jwt = new JwtSecurityToken(
            notBefore: DateTime.UtcNow,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(SECRET_KEY)),
                SecurityAlgorithms.HmacSha256));
        
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public static Guid GetId(this ClaimsPrincipal user) => Guid.Parse(user.FindFirst("id")!.Value);
}