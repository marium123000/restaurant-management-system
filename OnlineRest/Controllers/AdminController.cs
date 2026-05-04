using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRest.Models;

namespace OnlineRest.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "True";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            var totalOrders = await _context.Orders.CountAsync();
            var totalCustomers = await _context.Users.Where(u => !u.IsAdmin).CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var totalReservations = await _context.TableReservations.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var pendingReservations = await _context.TableReservations
                .Include(r => r.User)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.ReservationDate)
                .Take(5)
                .ToListAsync();

            var popularItems = await _context.OrderDetails
                .GroupBy(od => od.FoodItemId)
                .Select(g => new PopularItem
                {
                    FoodItemName = g.First().FoodItem!.Name,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            var model = new SalesReportVM
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                PendingOrders = pendingOrders,
                TotalReservations = totalReservations,
                PopularItems = popularItems,
                RecentOrders = recentOrders,
                PendingReservations = pendingReservations
            };

            return View(model);
        }

        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.Status = status;

            // Auto-update payment status when order is delivered
            if (status == "Delivered" && order.PaymentMethod == "Cash on Delivery")
            {
                order.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{id} status updated to {status}";
            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> FoodItems()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var foodItems = await _context.FoodItems
                .OrderBy(f => f.Category)
                .ThenBy(f => f.Name)
                .ToListAsync();

            return View(foodItems);
        }

        public IActionResult CreateFoodItem()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            ViewBag.Categories = new List<string> { "Appetizers", "Main Course", "Desserts", "Beverages" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFoodItem(FoodItem foodItem)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                foodItem.TotalSold = 0;
                _context.FoodItems.Add(foodItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Food item created successfully!";
                return RedirectToAction("FoodItems");
            }

            ViewBag.Categories = new List<string> { "Appetizers", "Main Course", "Desserts", "Beverages" };
            return View(foodItem);
        }

        public async Task<IActionResult> EditFoodItem(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
                return NotFound();

            ViewBag.Categories = new List<string> { "Appetizers", "Main Course", "Desserts", "Beverages" };
            return View(foodItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFoodItem(FoodItem foodItem)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _context.Entry(foodItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Food item updated successfully!";
                return RedirectToAction("FoodItems");
            }

            ViewBag.Categories = new List<string> { "Appetizers", "Main Course", "Desserts", "Beverages" };
            return View(foodItem);
        }

        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
                return NotFound();

            return View(foodItem);
        }

        [HttpPost, ActionName("DeleteFoodItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {
                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Food item deleted successfully!";
            }
            return RedirectToAction("FoodItems");
        }

        public async Task<IActionResult> Reservations()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservations = await _context.TableReservations
                .Include(r => r.User)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReservationStatus(int id, string status)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var reservation = await _context.TableReservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reservation #{id} status updated to {status}";
            return RedirectToAction("Reservations");
        }

        public async Task<IActionResult> Customers()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var customers = await _context.Users
                .Where(u => !u.IsAdmin)
                .Include(u => u.Orders)
                .OrderByDescending(u => u.LoyaltyPoints)
                .ToListAsync();

            return View(customers);
        }

        public async Task<IActionResult> SalesReport()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.FoodItem)
                .ToListAsync();

            var popularItems = await _context.OrderDetails
                .GroupBy(od => od.FoodItemId)
                .Select(g => new PopularItem
                {
                    FoodItemName = g.First().FoodItem!.Name,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var model = new SalesReportVM
            {
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                TotalCustomers = await _context.Users.Where(u => !u.IsAdmin).CountAsync(),
                PopularItems = popularItems,
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(20).ToList()
            };

            return View(model);
        }
    }
}