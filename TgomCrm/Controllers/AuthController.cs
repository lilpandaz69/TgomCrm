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

        // ✅ LOGIN
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

            // Generate JWT
            var token = GenerateToken(request.Username, role);

            // ✅ Save in server-side session
            HttpContext.Session.SetString("JwtToken", token);
            HttpContext.Session.SetString("Username", request.Username);
            HttpContext.Session.SetString("Role", role);

            // ✅ Also set cookie for persistence (so Angular can restore session)
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,              // set to false if testing locally without HTTPS
                SameSite = SameSiteMode.None, // required for cross-origin cookies
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new
            {
                Message = "Login successful",
                Role = role
            });
        }

        // ✅ CHECK CURRENT USER SESSION (for page reload)
        [HttpGet("me")]
        public IActionResult Me()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
            {
                // fallback: try reading from cookie
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { Message = "No active session" });

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);
                    username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                }
                catch
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }
            }

            return Ok(new { Username = username, Role = role });
        }

        // ✅ LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");
            return Ok(new { Message = "Logged out successfully" });
        }

        // 🔒 JWT Generator
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
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // 🧾 Request Model
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
