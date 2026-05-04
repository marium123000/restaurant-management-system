using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRest.Models;
using System.Security.Claims;

namespace OnlineRest.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Name),
                    new Claim("IsAdmin", user.IsAdmin.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                HttpContext.Session.SetInt32("LoyaltyPoints", user.LoyaltyPoints);
                
                // Load cart count from database
                if (!user.IsAdmin)
                {
                    var cartCount = await _context.CartItems
                        .Where(c => c.UserId == user.Id)
                        .SumAsync(c => c.Quantity);
                    HttpContext.Session.SetInt32("CartCount", cartCount);
                }
                else
                {
                    HttpContext.Session.SetInt32("CartCount", 0);
                }

                TempData["Success"] = $"Welcome back, {user.Name}!";

                if (user.IsAdmin)
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
                return RedirectToAction("Index", "Home");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.IsAdmin && model.SecretCode != "admin123")
            {
                ModelState.AddModelError("SecretCode", "Invalid admin secret code");
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                PhoneNumber = model.PhoneNumber,
                IsAdmin = model.IsAdmin,
                SecretCode = model.SecretCode,
                LoyaltyPoints = 100 // Welcome bonus!
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim("UserName", user.Name),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
            HttpContext.Session.SetInt32("LoyaltyPoints", user.LoyaltyPoints);
            HttpContext.Session.SetInt32("CartCount", 0);

            TempData["Success"] = $"Welcome {user.Name}! You've received 100 bonus loyalty points!";

            if (user.IsAdmin)
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login");

            // Prevent admin from accessing customer profile
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users don't have customer profiles.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(int id, string name, string email, string? phoneNumber, string? currentPassword, string? newPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value != id)
                return RedirectToAction("Login");

            // Prevent admin from updating customer profile
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users don't have customer profiles to update.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Check if email is already taken by another user
            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
            {
                TempData["Error"] = "Email already exists";
                return RedirectToAction("Profile");
            }

            // Update basic info
            user.Name = name;
            user.Email = email;
            user.PhoneNumber = phoneNumber;

            // Update password if provided
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (user.Password != currentPassword)
                {
                    TempData["Error"] = "Current password is incorrect";
                    return RedirectToAction("Profile");
                }
                user.Password = newPassword;
            }

            await _context.SaveChangesAsync();

            // Update session
            HttpContext.Session.SetString("UserName", user.Name);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}