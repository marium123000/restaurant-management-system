using OnlineRest.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineRest.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string ShippingAddress { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Processing, Out for Delivery, Delivered, Cancelled

        public string PaymentMethod { get; set; } = "Cash on Delivery"; // Cash on Delivery, Credit Card, Debit Card, Online Payment

        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        public int PointsEarned { get; set; } = 0; // Loyalty points earned from this order

        public int PointsRedeemed { get; set; } = 0; // Loyalty points used in this order

        public string? SpecialInstructions { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}