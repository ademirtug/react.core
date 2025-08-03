using duoword.admin.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.AI;



namespace duoword.admin.Server.Controllers
{
    [Route("/api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IAuthService auth;
        IChatClient chatGpt;

        public AuthController(IAuthService authService, IChatClient chat)
        {
            auth = authService;
            chatGpt = chat;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }
            var userEmail = User.Identity.Name;
            return Ok(new { user = userEmail });
        }

        [HttpPost("login")]
        public IActionResult Login([FromForm] string email, [FromForm] string password)
        {
            if (!auth.Login(email, password))
            {
                return Unauthorized("Credential Mismatch");
            }

            var jwt = auth.GenerateJWT(email, "Admin");
            var strToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            Response.Cookies.Append("access_token", strToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = jwt.ValidTo
            });
            return Ok(new { user = email, message = "Login successful" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Ok(new { message = "Logout successfull" });
        }

        [HttpGet("testai")]
        public async Task<IActionResult> TestAI()
        {
            //var resp = await chatGpt.GetResponseAsync("Hello, how are you?");
            //return Ok(new { message = "Logout successfull" });


            return Ok();
        }

    }
}
