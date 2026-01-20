using System.ComponentModel.DataAnnotations;



namespace Past2Future.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int? EventId { get; set; }     
        public Event? Event { get; set; }     // navigation

        public int UserId { get; set; }       // Foreign Key
        public User? User { get; set; }       // navigation propertys

        [Required(ErrorMessage = "Title cannot be empty.")]
        public string title { get; set; } = "";

        [Required(ErrorMessage = "Content cannot be empty.")]
        public string text { get; set; } = "";

        public bool isHistory { get; set; }
     

        public int? CountryId { get; set; }     
        public Country? Country { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
