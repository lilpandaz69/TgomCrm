using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tagom.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            string? role = null;

            if (request.Username == "owner" && request.Password == "samir123")
                role = "Owner";
            else if (request.Username == "staff" && request.Password == "tagomstaff123")
                role = "Staff";
            else
                return Unauthorized(new { message = "Authentication failed: invalid username or password." });

            var token = GenerateToken(request.Username, role);

            
            HttpContext.Session.SetString("JwtToken", token);
            HttpContext.Session.SetString("Username", request.Username);
            HttpContext.Session.SetString("Role", role);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
                Role = role
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            
            HttpContext.Session.Clear();
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("check-session")]
        public IActionResult CheckSession()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { Message = "No active session" });

            return Ok(new { Username = username, Role = role });
        }

        private string GenerateToken(string username, string role)
        {
            var jwtSettings = _config.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
