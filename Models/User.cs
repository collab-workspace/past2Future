using System;
using System.ComponentModel.DataAnnotations;

namespace Past2Future.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";


        public string Password { get; set; } = "";

        public string? Name { get; set; } = "";
        public string? Surname { get; set; } = "";
        public string? Country { get; set; } = "";

        public string? Username { get; set; } = "";
        public string? Gender { get; set; } = "";
        public DateTime? Birthdate { get; set; }

        public bool IsAdmin { get; set; }
        public string? LikedEventIds { get; set; }   


    }
}

