using System.ComponentModel.DataAnnotations;

namespace OnlineRest.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        public string Category { get; set; } = "Main Course"; // Appetizers, Main Course, Desserts, Beverages

        public int TotalSold { get; set; } = 0; // Track sales for top selling items

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}