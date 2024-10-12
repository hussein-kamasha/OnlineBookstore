using Microsoft.AspNetCore.Identity;

namespace Online_Bookstore.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
    }
}
