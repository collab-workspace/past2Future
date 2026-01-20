namespace Past2Future.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        public string Type { get; set; } = "";
        public string Message { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }   // FK
        public User? User { get; set; }   // navigation
    }
}

