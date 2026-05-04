using OnlineRest.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineRest.Models
{
    public class SalesReportVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public int TotalReservations { get; set; }
        public List<PopularItem> PopularItems { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public List<TableReservation> PendingReservations { get; set; } = new();
    }

    public class PopularItem
    {
        public string FoodItemName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Register as Admin")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Admin Secret Code")]
        public string? SecretCode { get; set; }
    }
}
