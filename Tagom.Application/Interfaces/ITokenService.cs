//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Tagom.Domain.Entities;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;

//namespace Tagom.Application.Interfaces
//{
//    public interface ITokenService
//    {
//        string CreateToken(User user);
//        DateTime GetExpiry();
//    }

//    public class JwtTokenService : ITokenService
//    {
//        private readonly IConfiguration _config;
//        private readonly string _key;
//        private readonly string _issuer;
//        private readonly string _audience;
//        private readonly int _durationMinutes;

//        public JwtTokenService(IConfiguration config)
//        {
//            _config = config;
//            _key = _config["Jwt:Key"]!;
//            _issuer = _config["Jwt:Issuer"]!;
//            _audience = _config["Jwt:Audience"]!;
//            _durationMinutes = int.Parse(_config["Jwt:DurationInMinutes"] ?? "60");
//        }

//        public string CreateToken(User user)
//        {
//            var claims = new List<Claim>
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
//            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
//            new Claim(ClaimTypes.Email, user.Email),
//            new Claim(ClaimTypes.Role, user.Role.ToString())
//        };

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: _issuer,
//                audience: _audience,
//                claims: claims,
//                expires: DateTime.UtcNow.AddMinutes(_durationMinutes),
//                signingCredentials: creds
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//        public DateTime GetExpiry() => DateTime.UtcNow.AddMinutes(_durationMinutes);
//    }
//}
