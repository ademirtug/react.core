using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace duoword.admin.Server.Services
{
    public interface IAuthService
    {
        bool Login(string username, string password);
        JwtSecurityToken GenerateJWT(string email, string? role);
    }


    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;

        private readonly TimeSpan accessTokenDuration = new TimeSpan(5, 0, 0, 0);
        private readonly TimeSpan refreshTokenDuration = new TimeSpan(180, 0, 0, 0);

        public AuthService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (username == "ademirtug@gmail.com" && password == "my_superb_secret_password_1234!")
            {
                return true;
            }
            return false;
        }

        public JwtSecurityToken GenerateJWT(string email, string? role)
        {
            var secretKey = configuration["JwtSettings:SecretKey"] ?? "";
            var issuer = configuration["JwtSettings:ValidIssuer"];
            var audience = configuration["JwtSettings:ValidAudience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            role ??= "Admin";

            var token = new JwtSecurityToken(issuer,
                audience,
                claims: new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                expires: DateTime.UtcNow.Add(accessTokenDuration),
                signingCredentials: credentials);

            return token;
        }
    }
}
