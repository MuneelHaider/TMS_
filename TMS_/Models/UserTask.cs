namespace TMS_.Models
{
    public class UserTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; } // Status can be "Pending", "InProgress", "Completed"

        // Foreign key for the user who created the task
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }

        // Foreign key for the user to whom the task is assigned
        public int? AssignedToId { get; set; }
        public User AssignedTo { get; set; }
    }
}
