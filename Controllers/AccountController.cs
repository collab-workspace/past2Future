using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Past2Future.Data;
using Past2Future.Models;


namespace Past2Future.Controllers;
// Authentication and account-related actions (login, register, logout).
public class AccountController : Controller
{
    private readonly AppDbContext db;

    public AccountController(AppDbContext context)
    {
        db = context;
    }



    public IActionResult Login()
    {
        return View();
    }
    // Authenticate user credentials and start a session.
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = db.Users
       .Where(u => u.Email == email && u.Password == password)
       .FirstOrDefault();


        if (user == null || user.Password != password)
        {
            TempData["LoginErr"] = "E-Mail or password is invalid. Please try again.";
            return View("Login");
        }

        // Session
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Username", user.Username ?? "");
        HttpContext.Session.SetString("IsAdmin", user.IsAdmin ? "true" : "false");




        if (user.IsAdmin)
        {

            return RedirectToAction("Index", "Events");
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View("Login"); 
    }
    // Create a new user account after basic validation (password match + bot-check).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(User newUser, string confirmPassword, int? robotAnswer)
    {
        if (!ModelState.IsValid)
            return View("Login", newUser);

        // Password confirmation
        if (newUser.Password != confirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return View("Login", newUser);
        }

        
        if (robotAnswer == null)
        {
            ModelState.AddModelError("", "Robot verification failed.");
            return View("Login", newUser);
        }

        // Email uniqueness
        if (db.Users.Any(u => u.Email == newUser.Email))
        {
            ModelState.AddModelError("Email", "This email is already registered.");
            return View("Login", newUser);
        }

        newUser.IsAdmin = false;
        db.Users.Add(newUser);
        db.SaveChanges();
        TempData["Success"] = "Registration completed. You can log in now.";
        return RedirectToAction("Login", "Account");
    }


    // End the current session and sign the user out.
    public IActionResult Logout()
    {
        TempData["Info"] = "You have been logged out.";
        return RedirectToAction("Login", "Account"); 
    }

    [HttpGet]
    public IActionResult EditProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login");

        var user = db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return RedirectToAction("Login");

        user.Password = ""; 
        return View(user);
    }
    // Update profile fields and optionally change password.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditProfile(
     User model,
     string CurrentPassword,
     string? NewPassword,
     string? ConfirmNewPassword)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login");

        var user = db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return RedirectToAction("Login");

   
        if (string.IsNullOrWhiteSpace(CurrentPassword) || user.Password != CurrentPassword)
        {
            TempData["Err"] = "Current password is incorrect. Changes could not saved.";
            model.Password = "";
            return View(model);
        }

 
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            if (string.IsNullOrWhiteSpace(ConfirmNewPassword) || NewPassword != ConfirmNewPassword)
            {
                TempData["Err"] = "New password and confirmation do not match.";
                model.Password = "";
                return View(model);
            }
        }

        user.Email = model.Email;
        user.Username = model.Username;
        user.Phone = model.Phone;
        user.Name = model.Name;
        user.Surname = model.Surname;
        user.Country = model.Country;
        user.Gender = model.Gender;
        user.Birthdate = model.Birthdate;

        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            user.Password = NewPassword; 
        }

        db.SaveChanges();

        HttpContext.Session.SetString("Username", user.Username ?? "");

        TempData["Success"] = "Your profile has been updated successfully.";
        return RedirectToAction("EditProfile");
    }






    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}