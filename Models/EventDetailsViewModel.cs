using Past2Future.Models;

namespace Past2Future.Models
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; }         
        public List<Comment> Comments { get; set; }
        public Comment NewComment { get; set; }     
    }
}