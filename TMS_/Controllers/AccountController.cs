using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // endpoint to register new user, checks if username is taken, hashes password, sets role, saves user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username is already taken.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "User"; // set role to "user"
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // endpoint to register new admin, only admins can call this, verifies admin creds, hashes password, sets role, saves admin
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

        // endpoint to login, checks username and password, returns user if valid
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

        // endpoint to get user profile by username, includes assigned and created tasks
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromHeader] string username)
        {
            var user = await _context.Users
                .Include(u => u.AssignedTasks)
                .Include(u => u.CreatedTasks)
                .SingleOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userProfile = new
            {
                user.Id,
                user.Username,
                user.Role,
                AssignedTasks = user.AssignedTasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Priority,
                    t.Status
                }),
                CreatedTasks = user.CreatedTasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Priority,
                    t.Status
                })
            };

            return Ok(userProfile);
        }

        // endpoint to register the first admin, checks if admin exists, then registers
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

        // endpoint to delete a user, only admins can do this, removes assigned tasks, deletes user
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromHeader] string adminUsername)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == adminUsername && u.Role == "Admin");
            if (admin == null)
            {
                return Unauthorized("Only admins can delete users.");
            }

            var user = await _context.Users.Include(u => u.AssignedTasks).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Role == "Admin")
            {
                return Unauthorized("Admins cannot delete other admins.");
            }

            foreach (var task in user.AssignedTasks)
            {
                _context.UserTasks.Remove(task);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }

        // endpoint to get all non-admin users
        [HttpGet("non-admin-users")]
        public async Task<IActionResult> GetNonAdminUsers()
        {
            var nonAdminUsers = await _context.Users
                .Where(u => u.Role != "Admin")
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role
                })
                .ToListAsync();

            return Ok(nonAdminUsers);
        }

        // endpoint for a user to delete their own account, removes assigned and created tasks
        [HttpDelete("delete-own-account/{username}")]
        public async Task<IActionResult> DeleteOwnAccount(string username)
        {
            var user = await _context.Users.Include(u => u.AssignedTasks).Include(u => u.CreatedTasks).SingleOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            _context.UserTasks.RemoveRange(user.AssignedTasks);
            _context.UserTasks.RemoveRange(user.CreatedTasks);
            _context.Users.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok("User deleted successfully.");
        }

        // endpoint to get user profile by username, returns user if found
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetUserProfile([FromHeader] string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            return Ok(user);
        }
    }
}
