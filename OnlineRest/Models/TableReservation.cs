using System.ComponentModel.DataAnnotations;

namespace OnlineRest.Models
{
    public class TableReservation
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public string ReservationTime { get; set; } // e.g., "18:00", "19:30"

        [Required]
        [Range(1, 20)]
        public int NumberOfGuests { get; set; }

        public string? SpecialRequests { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
