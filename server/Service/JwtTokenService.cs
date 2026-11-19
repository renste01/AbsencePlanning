using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infra.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Service;

//this method creates a token for whoever logs in and makes them able to automatically log in next session.
public class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(UserAccount user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"]
                     ?? throw new InvalidOperationException(
                         "Jwt:Secret is not configured. set Jwt_Secret in the deployment environment.");
        var issuer = jwtSection["Issuer"] ?? "AbsencePlanning"; //change name later
        var audience = jwtSection["Audience"] ?? "AbsencePlanning"; //also change later

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserAccountId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, user.RoleName)
        };

        if (user.RoleName == "Admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    
}