using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TMS_.Data;
using TMS_.Models;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpPost("assign")]
        public async Task<IActionResult> AssignTask([FromBody] TaskAssignmentDto taskDto)
        {
            var assignedToUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == taskDto.AssignedTo);
            if (assignedToUser == null)
            {
                return BadRequest("User not found.");
            }

            var createdByUser = await _context.Users.SingleOrDefaultAsync(u => u.Role == "Admin" && u.Username == taskDto.CreatedBy);
            if (createdByUser == null)
            {
                return BadRequest("Admin not found.");
            }

            var userTask = new UserTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Priority = taskDto.Priority,
                CreatedById = createdByUser.Id,
                AssignedToId = assignedToUser.Id,
                Status = "Pending"
            };

            _context.UserTasks.Add(userTask);
            await _context.SaveChangesAsync();

            return Ok("Task assigned successfully.");
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskStatusUpdateDto statusUpdateDto)
        {
            var userTask = await _context.UserTasks.FindAsync(statusUpdateDto.TaskId);
            if (userTask == null)
            {
                return NotFound("Task not found.");
            }

            userTask.Status = statusUpdateDto.Status;
            await _context.SaveChangesAsync();

            return Ok("Task status updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id, [FromHeader] string adminUsername)
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == adminUsername && u.Role == "Admin");
            if (admin == null)
            {
                return Unauthorized("Only admins can delete tasks.");
            }

            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok("Task deleted successfully.");
        }

        [HttpGet("task-counts")]
        public async Task<IActionResult> GetTaskCounts([FromHeader] string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.Role == "Admin")
            {
                var taskCounts = await _context.UserTasks
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(taskCounts);
            }
            else
            {
                var taskCounts = await _context.UserTasks
                    .Where(t => t.AssignedToId == user.Id)
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(taskCounts);
            }
        }

        [HttpGet("user-tasks")]
        public async Task<IActionResult> GetUserTasks([FromHeader] string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            if (user.Role == "Admin")
            {
                var tasks = await _context.UserTasks.ToListAsync();
                return Ok(tasks);
            }
            else
            {
                var tasks = await _context.UserTasks
                    .Where(t => t.AssignedToId == user.Id)
                    .ToListAsync();

                return Ok(tasks);
            }
        }

        [HttpGet("task-detail/{taskId}")]
        public async Task<IActionResult> GetTaskDetail(int taskId, [FromHeader] string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var task = await _context.UserTasks
                .SingleOrDefaultAsync(t => t.Id == taskId && t.AssignedToId == user.Id);
            if (task == null && user.Role != "Admin")
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }
    }

    public class TaskAssignmentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; } // Username of the assigned user
        public string CreatedBy { get; set; } // Username of the admin creating the task
    }

    public class TaskStatusUpdateDto
    {
        public int TaskId { get; set; }
        public string Status { get; set; }
    }
}
