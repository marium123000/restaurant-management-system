using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRest.Models;
using OnlineRest.Services;

namespace OnlineRest.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;

        public HomeController(AppDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index()
        {
            // Get top 6 selling items for homepage
            var topSellingItems = await _context.FoodItems
                .Where(f => f.IsAvailable)
                .OrderByDescending(f => f.TotalSold)
                .Take(6)
                .ToListAsync();

            ViewBag.TopSellingItems = topSellingItems;
            return View();
        }

        public async Task<IActionResult> Menu(string category = "All", string search = "")
        {
            var query = _context.FoodItems.Where(f => f.IsAvailable);

            // Search functionality - search across ALL categories
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Name.Contains(search) || f.Description.Contains(search) || f.Category.Contains(search));
                // When searching, reset category to "All" to show results from all categories
                category = "All";
            }
            // Filter by category only if not searching
            else if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(f => f.Category == category);
            }

            var foodItems = await query.OrderByDescending(f => f.TotalSold).ToListAsync();

            // Get categories for filter
            var categories = await _context.FoodItems
                .Where(f => f.IsAvailable)
                .Select(f => f.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.SearchTerm = search;

            return View(foodItems);
        }

        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from accessing customer reservations
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users cannot make reservations. Please use a customer account.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var reservations = await _context.TableReservations
                .Where(r => r.UserId == userId.Value)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(TableReservation reservation)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from creating reservations
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users cannot make reservations. Please use a customer account.";
                return RedirectToAction("Dashboard", "Admin");
            }

            reservation.UserId = userId.Value;
            reservation.Status = "Pending";
            reservation.CreatedAt = DateTime.Now;

            _context.TableReservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reservation request submitted successfully! We'll confirm shortly.";
            return RedirectToAction("Reservations");
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from canceling customer reservations
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users cannot cancel customer reservations from this page.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var reservation = await _context.TableReservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);

            if (reservation != null)
            {
                reservation.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reservation cancelled successfully.";
            }

            return RedirectToAction("Reservations");
        }

        public async Task<IActionResult> OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from accessing customer order history
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users should use the Admin Dashboard to view orders.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from viewing customer order details from this page
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users should use the Admin Dashboard to view order details.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.FoodItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Prevent admin from downloading customer receipts from this page
            if (HttpContext.Session.GetString("IsAdmin") == "True")
            {
                TempData["Error"] = "Admin users should use the Admin Dashboard to download receipts.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.FoodItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

            if (order == null)
                return NotFound();

            var pdfBytes = _pdfService.GenerateOrderReceipt(order);
            return File(pdfBytes, "application/pdf", $"Receipt_Order_{order.Id}.pdf");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}