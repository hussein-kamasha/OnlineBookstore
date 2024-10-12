namespace Online_Bookstore.Dtos
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int Quantity { get; set; }
    }
}
