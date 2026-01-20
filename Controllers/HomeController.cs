using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Past2Future.Data;
using Past2Future.Models;

namespace Past2Future.Controllers;

// Public pages: landing page, past/future lists, traveler thoughts, and feedback submission.
public class HomeController : Controller
{
    private readonly AppDbContext db;
    private readonly ILogger<HomeController> _logger;
    
    public static List<Feedback> feedbacks = new List<Feedback>();
    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        db = context;
    }
    // Landing page with the interactive 3D globe.
    public IActionResult Index()
    {
        
        if (HttpContext.Session.GetString("Username") == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var events = db.Events.Include(e => e.CountryRef).ToList();


        return View(events);
    }
    // Public list of future events.
    public IActionResult Future()
    {
        var now = DateTime.Now;

        var list = db.Events.Where(e => e.Date >= now).ToList();
        if (!list.Any())
        {
            TempData["Info"] = "Future event could not found.";
            return RedirectToAction("Index");
        }

        var randomEvent = list[new Random().Next(list.Count)];
        return RedirectToAction("Details", "Events", new { id = randomEvent.Id });
    }
    // Public list of past events.
    public IActionResult Past()
    {
        var now = DateTime.Now;

        var list = db.Events.Where(e => e.Date < now).ToList();
        if (!list.Any())
        {
            TempData["Info"] = "Past event could not found.";
            return RedirectToAction("Index");
        }

        var randomEvent = list[new Random().Next(list.Count)];
        return RedirectToAction("Details", "Events", new { id = randomEvent.Id });
    }



    public ActionResult Events()
    {
        var events = db.Events.ToList();
        return View(events);
    }

    public ActionResult Feedback()
    {
        return View();
    }

    public ActionResult AboutUs()
    {
        return View();
    }

    // Community page showing traveler thoughts (general comments not tied to a specific event).
    public IActionResult TravelersThoughts()
    {
        ViewBag.Countries = db.Countries.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
        var list = db.Comments.Include(c => c.User).Where(c => c.EventId == null)               
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return View(list);
    }
    // Show the form for posting a new traveler thought.
    [HttpGet]
    public IActionResult TravelerForm()
    {
        ViewBag.Countries = db.Countries
        .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
        .ToList();
        var list = db.Comments
            .Include(c => c.User)
            .Where(c => c.EventId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return View(list);
    }
    // Persist a traveler thought (requires authentication).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TravelersThought(Comment comment)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("TravelerForm");

        int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId == 0) return RedirectToAction("Login", "Account");

        comment.UserId = userId;
        comment.EventId = null;               
        comment.CreatedAt = DateTime.Now;

        db.Comments.Add(comment);
        db.SaveChanges();

        return RedirectToAction("TravelersThoughts");
    }


    // Store user feedback submitted from the feedback page.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitFeedback(Feedback fb)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }
        fb.UserId = user.UserId;
        fb.CreatedAt = DateTime.Now;

        db.Feedbacks.Add(fb);
        db.SaveChanges();

        TempData["Success"] = "Feedback sent succesfullly.";
        return RedirectToAction("Index", "Home");
    }
    // Delete a traveler thought (typically admin-only or owner-only depending on your rules).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteTravelerComment(int id)
    {

        if (HttpContext.Session.GetString("IsAdmin") != "true")
            return RedirectToAction("Index");

        var comment = db.Comments.FirstOrDefault(c => c.Id == id);
        if (comment != null)
        {
            int? eventId = comment.EventId;

            db.Comments.Remove(comment);
            db.SaveChanges();

            // event comment
            if (eventId != null)
                return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // general comment - travelers thought
        return RedirectToAction("TravelersThoughts");
    }





    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

