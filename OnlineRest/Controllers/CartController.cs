using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRest.Models;
using OnlineRest.Services;

namespace OnlineRest.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PdfService _pdfService;

        public CartController(AppDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "True";
        }

        private async Task<List<CartItem>> GetCart()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return new List<CartItem>();

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();

            return cartItems;
        }

        private async Task UpdateCartCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                HttpContext.Session.SetInt32("CartCount", 0);
                return;
            }

            var count = await _context.CartItems
                .Where(c => c.UserId == userId.Value)
                .SumAsync(c => c.Quantity);

            HttpContext.Session.SetInt32("CartCount", count);
        }

        public async Task<IActionResult> Index()
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot use the shopping cart.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var cart = await GetCart();
            ViewBag.SubTotal = cart.Sum(item => item.Price * item.Quantity);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int foodItemId, int quantity = 1)
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot add items to cart.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var foodItem = await _context.FoodItems.FindAsync(foodItemId);
            if (foodItem == null || !foodItem.IsAvailable)
                return NotFound();

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.FoodItemId == foodItemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId.Value,
                    FoodItemId = foodItem.Id,
                    FoodItemName = foodItem.Name,
                    Price = foodItem.Price,
                    Quantity = quantity,
                    ImageUrl = foodItem.ImageUrl ?? "/images/placeholder.jpg",
                    CreatedAt = DateTime.Now
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            await UpdateCartCount();

            TempData["Success"] = $"{foodItem.Name} added to cart!";
            return RedirectToAction("Menu", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int foodItemId, int quantity)
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot update cart.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.FoodItemId == foodItemId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                    TempData["Success"] = "Item removed from cart.";
                }
                else
                {
                    item.Quantity = quantity;
                    TempData["Success"] = "Cart updated successfully.";
                }
                await _context.SaveChangesAsync();
                await UpdateCartCount();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromCart(int foodItemId)
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot remove items from cart.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var item = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.FoodItemId == foodItemId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                await UpdateCartCount();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ClearCart()
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot clear cart.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetInt32("CartCount", 0);

            TempData["Success"] = "Cart cleared successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot checkout.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var cart = await GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Menu", "Home");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId);

            ViewBag.SubTotal = cart.Sum(item => item.Price * item.Quantity);
            ViewBag.LoyaltyPoints = user?.LoyaltyPoints ?? 0;
            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string shippingAddress, string paymentMethod, int pointsToRedeem = 0, string? specialInstructions = null)
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot checkout.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            // Validate payment method is selected
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Please select a payment method!";
                return RedirectToAction("Checkout");
            }

            var cart = await GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Menu", "Home");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound();

            // Validate points redemption
            if (pointsToRedeem > user.LoyaltyPoints)
            {
                TempData["Error"] = "You don't have enough loyalty points!";
                return RedirectToAction("Checkout");
            }

            // Calculate totals
            var subtotal = cart.Sum(item => item.Price * item.Quantity);
            var discount = pointsToRedeem * 0.01m; // 1 point = $0.01
            var totalAmount = subtotal - discount;

            // Determine payment status based on payment method
            string paymentStatus = "Pending"; // Default for Cash on Delivery
            if (paymentMethod == "Credit Card" || paymentMethod == "Online Payment")
            {
                // Card and online payments are processed immediately
                paymentStatus = "Paid";
            }

            // Create order
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                ShippingAddress = shippingAddress,
                TotalAmount = totalAmount,
                Status = "Pending",
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                PointsRedeemed = pointsToRedeem,
                PointsEarned = (int)Math.Floor(totalAmount), // Earn 1 point per $1
                SpecialInstructions = specialInstructions
            };

            _context.Orders.Add(order);
            
            // Save order first to get the OrderId
            await _context.SaveChangesAsync();

            // Add order details and update food item sales
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    FoodItemId = item.FoodItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);

                // Update total sold count
                var foodItem = await _context.FoodItems.FindAsync(item.FoodItemId);
                if (foodItem != null)
                {
                    foodItem.TotalSold += item.Quantity;
                }
            }

            // Update user loyalty points
            user.LoyaltyPoints -= pointsToRedeem;
            user.LoyaltyPoints += order.PointsEarned;
            HttpContext.Session.SetInt32("LoyaltyPoints", user.LoyaltyPoints);

            await _context.SaveChangesAsync();

            // Clear cart from database
            var userCartItems = await _context.CartItems
                .Where(c => c.UserId == userId.Value)
                .ToListAsync();
            _context.CartItems.RemoveRange(userCartItems);
            await _context.SaveChangesAsync();
            
            HttpContext.Session.SetInt32("CartCount", 0);

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot view order confirmations.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

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
            if (IsAdmin())
            {
                TempData["Error"] = "Admin users cannot download customer receipts from this page.";
                return RedirectToAction("Dashboard", "Admin");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

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
    }
}