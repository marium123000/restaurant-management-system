namespace OnlineRest.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public FoodItem? FoodItem { get; set; }
    }
}