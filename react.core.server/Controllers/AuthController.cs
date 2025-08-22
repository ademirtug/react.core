using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace react.core.Server.Controllers
{
	[Route("api/v1/[controller]")]
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
			//if (email != "demo@example.com" || password != "1234")
			//{
			//	return Unauthorized();
			//}

			var token = GenerateJwt(email, "Admin");
			var stringToken = new JwtSecurityTokenHandler().WriteToken(token);
			Response.Cookies.Append("access_token", stringToken, new CookieOptions
			{
				HttpOnly = true, // Prevents JavaScript access (XSS protection)
				Secure = true,   // Requires HTTPS
				SameSite = SameSiteMode.Strict, // Protects against CSRF
				Expires = DateTime.UtcNow.AddHours(1)
			});

			return Ok(new
			{
				token = stringToken,
				user = new { email }
			});
		}

		[HttpPost("logout")]
		public IActionResult Logout()
		{
			return Ok(new { message = "Logout successful" });
		}

		[HttpGet("me")]
		public IActionResult GetCurrentAdmin()
		{
			// For demo purposes, return a dummy user
			return Ok(new
			{
				email = "demo@example.com",
				role = "Admin"
			});
		}

		[HttpPost("refresh")]
		public IActionResult RefreshToken()
		{
			// Just generate a new token with same claims
			var email = User.FindFirstValue(ClaimTypes.Email) ?? "demo@example.com";
			var newToken = GenerateJwt(email, "Admin");

			return Ok(new { token = newToken });
		}

		private JwtSecurityToken GenerateJwt(string email, string? role)
		{
			var securityKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Role, role ?? "Admin") // Default role
        };

			var token = new JwtSecurityToken(
				issuer: _configuration["JwtSettings:Issuer"],
				audience: _configuration["JwtSettings:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(1), // Token valid for 1 hour
				signingCredentials: credentials
			);

			return token;
		}
	}
}
