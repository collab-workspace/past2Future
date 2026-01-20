using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Past2Future.Data;
using Past2Future.Models;

namespace Past2Future.Controllers
{
    // Admin-only management pages (feedback moderation and country reference data).
    public class AdminController : Controller
    {
        private readonly AppDbContext db;
        public AdminController(AppDbContext context)
        {
            db = context;
        }
        // Review feedback messages submitted by users (admin only
        public IActionResult Feedbacks()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Index", "Home");

            var list = db.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            return View(list);
        }
        // Delete a feedback entry (admin only).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFeedback(int id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Index", "Home");

            var feedback = db.Feedbacks.FirstOrDefault(f => f.Id == id);
            if (feedback != null)
            {
                db.Feedbacks.Remove(feedback);
                db.SaveChanges();
            }

            return RedirectToAction("Feedbacks");
        }


        // Manage country reference data used by events (admin only).
        public IActionResult Countries()
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                var list = db.Countries
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(list);
            }

            public IActionResult CreateCountry()
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                return View(new Country());
            }
            // Add a new country entry (admin only).
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult CreateCountry(Country newCountry)
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                newCountry.Name = (newCountry.Name ?? "").Trim();

                if (string.IsNullOrWhiteSpace(newCountry.Name))
                {
                    ModelState.AddModelError("Name", "Country name can not be empty");
                    return View(newCountry);
                }

                var exists = db.Countries.Any(c => c.Name.ToLower() == newCountry.Name.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "You already have that country.");
                    return View(newCountry);
                }

                db.Countries.Add(newCountry);
                db.SaveChanges();

                TempData["Success"] = "Country added!";
                return RedirectToAction("Countries");
            }

            public IActionResult EditCountry(int id)
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                var country = db.Countries.FirstOrDefault(c => c.Id == id);
                if (country == null) return NotFound();

                return View(country);
            }
            // Update an existing country entry (admin only).
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult EditCountry(int id, Country model)
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                var country = db.Countries.FirstOrDefault(c => c.Id == id);
                if (country == null) return NotFound();

                var newName = (model.Name ?? "").Trim();
                if (string.IsNullOrWhiteSpace(newName))
                {
                    ModelState.AddModelError("Name", "Country name can not be empty");
                    return View(model);
                }

                var exists = db.Countries.Any(c => c.Id != id && c.Name.ToLower() == newName.ToLower());
                if (exists)
                {
                    ModelState.AddModelError("Name", "You already have that country");
                    return View(model);
                }

                country.Name = newName;
                country.FlagImageUrl = model.FlagImageUrl?.Trim();
                country.IsActive = model.IsActive;

                db.SaveChanges();

                TempData["Success"] = "Country updated";
                return RedirectToAction("Countries");
            }

            // Activate/deactivate a country without deleting it (admin only).
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult ToggleCountry(int id)
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");

                var country = db.Countries.FirstOrDefault(c => c.Id == id);
                if (country == null) return NotFound();

                country.IsActive = !country.IsActive;
                db.SaveChanges();

                return RedirectToAction("Countries");
            }
            // Permanently delete a country entry (admin only).
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult DeleteCountry(int id)
            {
                if (HttpContext.Session.GetString("IsAdmin") != "true")
                    return RedirectToAction("Index", "Home");


                var country = db.Countries.FirstOrDefault(c => c.Id == id);
                if (country != null)
                {
                    db.Countries.Remove(country);
                    db.SaveChanges();
                    TempData["Success"] = "Country deleted";
                }

                return RedirectToAction("Countries");
            }

}
}
