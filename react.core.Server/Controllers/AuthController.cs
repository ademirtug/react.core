using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;




namespace react.core.Server.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string email, [FromForm] string password)
        {
            // Skip actual authentication - just generate token for any request
            var token = GenerateJwtToken(email);

            return Ok(new
            {
                token = token,
                user = new
                {
                    email = email,
                    role = "User" // Default role
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Just return success - token invalidation would be handled client-side
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // For demo purposes, return a dummy user
            return Ok(new
            {
                email = "demo@example.com",
                role = "User"
            });
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken()
        {
            // Just generate a new token with same claims
            var email = User.FindFirstValue(ClaimTypes.Email) ?? "demo@example.com";
            var newToken = GenerateJwtToken(email);

            return Ok(new { token = newToken });
        }

        private string GenerateJwtToken(string email)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "User") // Default role
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token valid for 1 hour
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
