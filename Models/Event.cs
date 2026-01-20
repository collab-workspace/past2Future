using System;
using System.ComponentModel.DataAnnotations;

namespace Past2Future.Models 
{
    public class Event
    {
        [Key]
        public int Id { get; set; } 

        public string Title { get; set; }      
        public string Description { get; set; } 
        public string Type { get; set; }        
        public string? Country { get; set; }     
        public double Latitude { get; set; }    
        public double Longitude { get; set; }   
        public DateTime Date { get; set; }
        public string? ImageUrl { get; set; } = "";
        public int LikeCount { get; set; } = 0;

        public int? CountryId { get; set; }
        public Country? CountryRef { get; set; }




    }
}