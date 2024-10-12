namespace Online_Bookstore.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign Key to the User
        public User User { get; set; } // Navigation property to the User
        public ICollection<CartItem> CartItems { get; set; }
    }
}
