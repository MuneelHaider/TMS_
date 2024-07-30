namespace TMS_.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User";

        public ICollection<UserTask> AssignedTasks { get; set; } = new List<UserTask>();
        public ICollection<UserTask> CreatedTasks { get; set; } = new List<UserTask>();
    }
}
