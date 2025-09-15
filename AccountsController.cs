using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewProject.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly DashboardKpiContext _context;
        private readonly IConfiguration _config;

        public AccountsController(DashboardKpiContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login login)
        {
            if (login == null) return BadRequest();

            var user = _context.Users.SingleOrDefault(u => 
                u.Username == login.Username && u.Password == login.Password);

            if (user == null)
                return Unauthorized(new { Message = "Invalid credentials" });

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new AuthResponse
            {
                Username = user.Username,
                Role = user.Role,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
