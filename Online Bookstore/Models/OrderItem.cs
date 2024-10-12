using Online_Bookstore.Models;
using System.Text.Json.Serialization;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; }

    [JsonIgnore] // Ignore this to avoid serialization cycle
    public int OrderId { get; set; }
    public Order Order { get; set; }
}