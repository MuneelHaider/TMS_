using Microsoft.AspNetCore.Mvc;
using TMS_.Data;
using TMS_.Models;
using BCrypt.Net;
using System.Linq;
using System.Threading.Tasks;

namespace TMS_.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly TMS_DbContext _context;

        public AccountController(TMS_DbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username is already taken.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "User"; // Ensure role is always set to "User"
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(
            [FromBody] User user,
            [FromHeader(Name = "adminUsername")] string adminUsername,
            [FromHeader(Name = "adminPassword")] string adminPassword)
        {
            var admin = _context.Users.FirstOrDefault(u => u.Username == adminUsername && u.Role == "Admin");
            if (admin == null || !BCrypt.Net.BCrypt.Verify(adminPassword, admin.Password))
            {
                return Unauthorized("Only admins can add new admins.");
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username is already taken.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "Admin";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Admin registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == loginUser.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(new { Message = "Login successful", User = user });
        }

        [HttpPost("register-initial-admin")]
        public async Task<IActionResult> RegisterInitialAdmin([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Role == "Admin"))
            {
                return BadRequest("An admin already exists.");
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username is already taken.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "Admin";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Initial admin registered successfully.");
        }
    }
}
