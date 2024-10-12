using Online_Bookstore.Models;
using System.Text.Json.Serialization;

public class Order
{
    public int Id { get; set; }
    public decimal TotalPrice { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string RecipientName { get; set; } 
    public string ShippingAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}
