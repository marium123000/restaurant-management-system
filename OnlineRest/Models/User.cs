using System.ComponentModel.DataAnnotations;

namespace OnlineRest.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool IsAdmin { get; set; } = false;

        public string? SecretCode { get; set; } // For admin registration

        public int LoyaltyPoints { get; set; } = 0; // Loyalty rewards system

        public string? PhoneNumber { get; set; }

        public ICollection<Order>? Orders { get; set; }
        public ICollection<TableReservation>? Reservations { get; set; }
    }
}