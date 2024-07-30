namespace TMS_.Models
{
    public class UserTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }

        public string Status { get; set; } = "Pending"; // set default to pending

        // foreign key for the user who created the task
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }

        // foreign key for the user to whom the task is assigned
        public int? AssignedToId { get; set; }
        public User AssignedTo { get; set; }
    }
}
