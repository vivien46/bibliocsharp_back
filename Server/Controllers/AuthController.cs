using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Database;
using System.Security.Cryptography;
using System.Text;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/user/")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

            var hashedBytes = SHA512.HashData(Encoding.UTF8.GetBytes(password));
            var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

            if (user == null || user.PasswordHash != hashedPassword)
                return BadRequest("Invalid username or password");

            if (user != null && user.PasswordHash == hashedPassword)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username.ToString());
                HttpContext.Session.SetString("Role", user.Role.ToString());
                return Ok(user);
            }
            return Unauthorized();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username== null)
                return Unauthorized();
        
            var user = await _context.Users.FindAsync(username);
            return Ok(user);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // HttpContext.Session.Remove("UserId");
            // HttpContext.Session.Remove("Username");
            HttpContext.Session.Clear();
            return Ok();
        }

    }
}
            