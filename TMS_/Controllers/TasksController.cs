using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS_.Data;
using TMS_.Models;

namespace TMS_.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TMS_DbContext _context;

        public TasksController(TMS_DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return Unauthorized();
            }

            var tasks = role == "Admin"
                ? await _context.UserTasks.ToListAsync()
                : await _context.UserTasks
                    .Where(t => t.AssignedToId == int.Parse(userId) || t.CreatedById == int.Parse(userId))
                    .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] UserTask task)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Unauthorized();
            }

            task.CreatedById = int.Parse(userId);
            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] int assignedToId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "Admin")
            {
                return Unauthorized();
            }

            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedToId = assignedToId;
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] string status)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Unauthorized();
            }

            var task = await _context.UserTasks.FindAsync(id);
            if (task == null || task.AssignedToId != int.Parse(userId))
            {
                return Unauthorized();
            }

            task.Status = status;
            await _context.SaveChangesAsync();
            return Ok(task);
        }
    }
}
