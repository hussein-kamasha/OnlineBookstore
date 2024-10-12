namespace Online_Bookstore.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int BookId { get; set; } // Foreign Key to Book
        public Book Book { get; set; } // Navigation property to Book
        public int Quantity { get; set; }
        public int ShoppingCartId { get; set; } // Foreign Key to ShoppingCart
        public ShoppingCart ShoppingCart { get; set; } // Navigation property to ShoppingCart
    }
}
