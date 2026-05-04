using Microsoft.EntityFrameworkCore;
using OnlineRest.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OnlineRest.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<TableReservation> TableReservations { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.FoodItem)
                .WithMany(f => f.OrderDetails)
                .HasForeignKey(od => od.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TableReservation>()
                .HasOne(tr => tr.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(tr => tr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Admin", Email = "admin@restaurant.com", Password = "Admin123@", IsAdmin = true, SecretCode = "admin123", LoyaltyPoints = 0, PhoneNumber = "+92-300-1234567" },
                new User { Id = 2, Name = "Marium", Email = "customer@gmail.com", Password = "Customer123@", IsAdmin = false, LoyaltyPoints = 150, PhoneNumber = "+92-321-9876543" },
                new User { Id = 3, Name = "M.Hashir", Email = "hashir@gmail.com", Password = "!1@2#3", IsAdmin = false, LoyaltyPoints = 250, PhoneNumber = "+92-333-5555555" }
            );

            // Seed food items with categories
            modelBuilder.Entity<FoodItem>().HasData(
                // Appetizers
                new FoodItem { Id = 1, Name = "Chicken Wings", Description = "Spicy buffalo chicken wings with ranch dip", Price = 8.99m, ImageUrl = "/images/chicken-wings.jpg", IsAvailable = true, Category = "Appetizers", TotalSold = 145 },
                new FoodItem { Id = 2, Name = "Garlic Bread", Description = "Toasted bread with garlic butter and herbs", Price = 4.99m, ImageUrl = "/images/garlic-bread.jpg", IsAvailable = true, Category = "Appetizers", TotalSold = 230 },
                new FoodItem { Id = 3, Name = "Spring Rolls", Description = "Crispy vegetable spring rolls with sweet chili sauce", Price = 6.99m, ImageUrl = "/images/spring-rolls.jpg", IsAvailable = true, Category = "Appetizers", TotalSold = 180 },
                
                // Main Course
                new FoodItem { Id = 4, Name = "Chicken Biryani", Description = "Aromatic basmati rice with tender chicken pieces and spices", Price = 12.99m, ImageUrl = "/images/chicken-biryani.jpg", IsAvailable = true, Category = "Main Course", TotalSold = 520 },
                new FoodItem { Id = 5, Name = "Zinger Burger", Description = "Crispy fried chicken fillet in a soft bun with fresh veggies", Price = 9.99m, ImageUrl = "/images/zinger-burger.jpg", IsAvailable = true, Category = "Main Course", TotalSold = 450 },
                new FoodItem { Id = 6, Name = "Alfredo Pasta", Description = "Creamy white sauce pasta with grilled chicken", Price = 11.99m, ImageUrl = "/images/alfredo-pasta.jpg", IsAvailable = true, Category = "Main Course", TotalSold = 380 },
                new FoodItem { Id = 7, Name = "Beef Steak", Description = "Grilled beef steak with mashed potatoes and vegetables", Price = 18.99m, ImageUrl = "/images/beef-steak.jpg", IsAvailable = true, Category = "Main Course", TotalSold = 290 },
                new FoodItem { Id = 8, Name = "Margherita Pizza", Description = "Classic pizza with tomato sauce, mozzarella, and basil", Price = 13.99m, ImageUrl = "/images/margherita-pizza.jpg", IsAvailable = true, Category = "Main Course", TotalSold = 410 },
                
                // Desserts
                new FoodItem { Id = 9, Name = "Chocolate Lava Cake", Description = "Warm chocolate cake with molten center and vanilla ice cream", Price = 7.99m, ImageUrl = "/images/chocolate-lava-cake.jpg", IsAvailable = true, Category = "Desserts", TotalSold = 320 },
                new FoodItem { Id = 10, Name = "Cheesecake", Description = "New York style cheesecake with berry compote", Price = 6.99m, ImageUrl = "/images/cheesecake.jpg", IsAvailable = true, Category = "Desserts", TotalSold = 275 },
                new FoodItem { Id = 11, Name = "Tiramisu", Description = "Classic Italian dessert with coffee and mascarpone", Price = 8.99m, ImageUrl = "/images/tiramisu.jpg", IsAvailable = true, Category = "Desserts", TotalSold = 190 },
                
                // Beverages
                new FoodItem { Id = 12, Name = "Fresh Orange Juice", Description = "Freshly squeezed orange juice", Price = 4.99m, ImageUrl = "/images/orange-juice.jpg", IsAvailable = true, Category = "Beverages", TotalSold = 340 },
                new FoodItem { Id = 13, Name = "Iced Coffee", Description = "Cold brew coffee with ice and cream", Price = 5.99m, ImageUrl = "/images/iced-coffee.jpg", IsAvailable = true, Category = "Beverages", TotalSold = 390 },
                new FoodItem { Id = 14, Name = "Mango Smoothie", Description = "Thick mango smoothie with yogurt", Price = 6.99m, ImageUrl = "/images/mango-smoothie.jpg", IsAvailable = true, Category = "Beverages", TotalSold = 260 },
                new FoodItem { Id = 15, Name = "Soft Drink", Description = "Choice of Coke, Sprite, or Fanta", Price = 2.99m, ImageUrl = "/images/soft-drink.jpg", IsAvailable = true, Category = "Beverages", TotalSold = 580 }
            );

            // Seed orders
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, UserId = 3, OrderDate = new DateTime(2026, 4, 22, 21, 58, 15), ShippingAddress = "House 123, Block A, Korangi, Karachi", TotalAmount = 45.96m, Status = "Delivered", PaymentMethod = "Cash on Delivery", PaymentStatus = "Paid", PointsEarned = 46, PointsRedeemed = 0 },
                new Order { Id = 2, UserId = 2, OrderDate = new DateTime(2026, 4, 27, 22, 30, 40), ShippingAddress = "Flat 5B, Gulshan-e-Iqbal, Karachi", TotalAmount = 24.98m, Status = "Delivered", PaymentMethod = "Credit Card", PaymentStatus = "Paid", PointsEarned = 25, PointsRedeemed = 0 },
                new Order { Id = 3, UserId = 2, OrderDate = new DateTime(2026, 5, 1, 19, 16, 55), ShippingAddress = "Office 301, Clifton, Karachi", TotalAmount = 32.97m, Status = "Out for Delivery", PaymentMethod = "Online Payment", PaymentStatus = "Paid", PointsEarned = 33, PointsRedeemed = 50 }
            );

            // Seed order details
            modelBuilder.Entity<OrderDetail>().HasData(
                new OrderDetail { Id = 1, OrderId = 1, FoodItemId = 4, Quantity = 2, UnitPrice = 12.99m },
                new OrderDetail { Id = 2, OrderId = 1, FoodItemId = 5, Quantity = 1, UnitPrice = 9.99m },
                new OrderDetail { Id = 3, OrderId = 1, FoodItemId = 9, Quantity = 1, UnitPrice = 7.99m },
                new OrderDetail { Id = 4, OrderId = 1, FoodItemId = 13, Quantity = 1, UnitPrice = 5.99m },
                new OrderDetail { Id = 5, OrderId = 2, FoodItemId = 6, Quantity = 1, UnitPrice = 11.99m },
                new OrderDetail { Id = 6, OrderId = 2, FoodItemId = 12, Quantity = 2, UnitPrice = 4.99m },
                new OrderDetail { Id = 7, OrderId = 2, FoodItemId = 10, Quantity = 1, UnitPrice = 6.99m },
                new OrderDetail { Id = 8, OrderId = 3, FoodItemId = 8, Quantity = 1, UnitPrice = 13.99m },
                new OrderDetail { Id = 9, OrderId = 3, FoodItemId = 7, Quantity = 1, UnitPrice = 18.99m }
            );
        }
    }
}