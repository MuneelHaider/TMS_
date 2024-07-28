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
            // Check for duplicate usernames
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
