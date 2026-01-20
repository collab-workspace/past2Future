using System.ComponentModel.DataAnnotations;

namespace Past2Future.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string? FlagImageUrl { get; set; }   

        public bool IsActive { get; set; } = true;
    }
}
