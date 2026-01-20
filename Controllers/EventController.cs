using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Past2Future.Data;   
using Past2Future.Models; 
using System.Linq;
using System.Threading.Tasks;

namespace Past2Future.Controllers
{
    // Event management and public event pages (list, details, comments, likes, and JSON feed for the globe).

    public class EventsController : Controller
    {
        private readonly AppDbContext db;

        public EventsController(AppDbContext context)
        {
            db = context;
        }

        private bool IsAdmin()
    => HttpContext.Session.GetString("IsAdmin") == "true";


        // List all events (admin can manage; public can browse).
        public IActionResult Index()
        {
                var events = db.Events.Include(e => e.CountryRef).ToList();

            return View(events);
        }


        // Show the event creation form (admin only).
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View();
        }


        // Create a new event and save it to the database.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Event model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.Countries = db.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            var selectedCountry = db.Countries.Find(model.CountryId);
            if (selectedCountry != null)
            {
                model.Country = selectedCountry.Name; 
            }

            if (model.CountryId <=0)
                ModelState.AddModelError("CountryId", "Please select a country");

            if (ModelState.IsValid)
            {
                // LikeCount initially zero
                model.LikeCount = 0;

               

                db.Events.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Show the edit form for an existing event (admin only).
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");
            var editedEvent = db.Events
                .Include(e => e.CountryRef)
                .FirstOrDefault(e => e.Id == id);

            if (editedEvent == null) return NotFound();

            ViewBag.Countries = db.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            
            return View(editedEvent);
        }

        // Update an existing event in the database (admin only).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Event model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (model.CountryId == null || model.CountryId <= 0)
                ModelState.AddModelError("CountryId", "Please select a country");

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
                return View(model);
            }

            var ev = db.Events.FirstOrDefault(e => e.Id == model.Id);
            if (ev == null) return NotFound();

          
            ev.Title = model.Title;
            ev.Description = model.Description;
            ev.Type = model.Type;
            ev.Date = model.Date;
            ev.Latitude = model.Latitude;
            ev.Longitude = model.Longitude;
            ev.ImageUrl = model.ImageUrl;

            ev.CountryId = model.CountryId;

           
            var selectedCountry = db.Countries.FirstOrDefault(c => c.Id == model.CountryId);
            ev.Country = selectedCountry?.Name;

            db.SaveChanges();

            TempData["Success"] = "Event updated.";
            return RedirectToAction("Index");
        }


        // Delete an event (admin only).
        public IActionResult Delete(int id)

        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");
            var eventToDelete = db.Events.Find(id);
            if (eventToDelete != null)
            {
                db.Events.Remove(eventToDelete);
                db.SaveChanges();
            }
            TempData["Warning"] = "Event deleted.";
            return RedirectToAction(nameof(Index));
        }

        // Display event details along with its comments and like state.
        public IActionResult Details(int? id)
        {
  
            // Take the event together with its related country
            var eventItem = db.Events
                .Include(e => e.CountryRef)
                .FirstOrDefault(e => e.Id == id.Value);
            // Return 404 if the event does not exist
            if (eventItem == null)
                return NotFound();
            // Store country id for comment creation
            int? countryId = eventItem.CountryRef?.Id;
            // Get only comments related to this even
            var eventComments = db.Comments.Include(c => c.User).Where(c => c.EventId == id.Value)   
                .OrderByDescending(c => c.CreatedAt)
                .ToList();


            var viewModel = new EventDetailsViewModel
            {
                Event = eventItem,
                Comments = eventComments,
                NewComment = new Comment
                {
                    EventId = id.Value,
                    CountryId = countryId ?? 0
                }
            };
            // Check if the current user has already liked this event
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            bool alreadyLiked = false;

            if (userId != 0)
            {
                var user = db.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null && !string.IsNullOrEmpty(user.LikedEventIds))
                {
                    alreadyLiked = user.LikedEventIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Contains(eventItem.Id.ToString());
                }
            }
            // Pass like status to the view
            ViewBag.AlreadyLiked = alreadyLiked;

            return View(viewModel);
        }




        // Add a new comment to an event.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComment(EventDetailsViewModel model)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0) return RedirectToAction("Login", "Account");

            var commentToAdd = model.NewComment;
            if (commentToAdd == null) return RedirectToAction("Index", "Home");

            commentToAdd.UserId = userId;
            commentToAdd.CreatedAt = DateTime.Now;

            db.Comments.Add(commentToAdd);
            db.SaveChanges();
            TempData["Success"] = "Your comment has been added.";
            return RedirectToAction("Details", new { id = commentToAdd.EventId });
        }
        // Toggle/record a like for the current user and update the event like count.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LikeEvent(int eventId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            var likedEvent = db.Events.FirstOrDefault(e => e.Id == eventId);
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);



            // check if its already liked
            if (!string.IsNullOrEmpty(user.LikedEventIds) &&
                user.LikedEventIds.Split(',').Contains(eventId.ToString()))
            {

                return RedirectToAction("Details", new { id = eventId });
            }

     
            likedEvent.LikeCount++;

            if (string.IsNullOrEmpty(user.LikedEventIds))
                user.LikedEventIds = eventId.ToString();
            else
                user.LikedEventIds += "," + eventId;

            db.SaveChanges();

            return RedirectToAction("Details", new { id = eventId });
        }



        // Provide a lightweight JSON feed used by the 3D globe to render pins.
        [HttpGet]
        public async Task<IActionResult> GetEventsJson()
        {
            // Select and send the data that frontend needs.
            var events = await db.Events.Select(e => new {
                e.Id,
                e.Title,
                e.Type,     
                e.Country,
                e.Latitude,  
                e.Longitude, 
                e.Description
            }).ToListAsync();

            return Json(events);
        }
       
        
    }
}