namespace TMS_.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // In a real app, use hashed passwords
        public string Role { get; set; } // Role can be "Admin" or "User"

        // Navigation property
        public ICollection<UserTask> AssignedTasks { get; set; }
        public ICollection<UserTask> CreatedTasks { get; set; }
    }
}
