using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Bookstore.Data;
using Online_Bookstore.Dtos;
using Online_Bookstore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace Online_Bookstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/shoppingcart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return Ok(new { CartItems = new List<CartItemDto>() });

            var cartItems = cart.CartItems.Select(ci => new CartItemDto
            {
                CartItemId = ci.Id,
                BookId = ci.BookId,
                BookTitle = ci.Book.Title,
                Quantity = ci.Quantity
            }).ToList();

            return Ok(cartItems);
        }

        // POST: api/shoppingcart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var book = await _context.Books.FindAsync(addToCartDto.BookId);
            if (book == null) return NotFound($"Book with ID {addToCartDto.BookId} not found.");

            if (addToCartDto.Quantity > book.AvailableQuantity)
                return BadRequest($"Insufficient stock for book ID {addToCartDto.BookId}.");

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId, CartItems = new List<CartItem>() };
                _context.ShoppingCarts.Add(cart);
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.BookId == addToCartDto.BookId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    BookId = addToCartDto.BookId,
                    Quantity = addToCartDto.Quantity
                });
            }

            book.AvailableQuantity -= addToCartDto.Quantity;
            await _context.SaveChangesAsync();

            return Ok("Book added to cart.");
        }

        // PUT: api/shoppingcart/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto updateCartItemDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .Include(ci => ci.ShoppingCart) // Include ShoppingCart to check the UserId
                .FirstOrDefaultAsync(ci => ci.Id == updateCartItemDto.CartItemId && ci.ShoppingCart.UserId == userId);

            if (cartItem == null) return NotFound();

            if (updateCartItemDto.Quantity > cartItem.Book.AvailableQuantity)
                return BadRequest($"Insufficient stock for book ID {cartItem.BookId}.");

            // Update the available quantity of the book
            cartItem.Book.AvailableQuantity += cartItem.Quantity - updateCartItemDto.Quantity;
            cartItem.Quantity = updateCartItemDto.Quantity;

            await _context.SaveChangesAsync();
            return Ok("Cart item updated.");
        }

        // DELETE: api/shoppingcart/remove/{id}
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Book)
                .Include(ci => ci.ShoppingCart) // Include ShoppingCart to check the UserId
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.ShoppingCart.UserId == userId);

            if (cartItem == null) return NotFound();

            // Restore the available quantity of the book
            cartItem.Book.AvailableQuantity += cartItem.Quantity;
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok("Item removed from cart.");
        }

        // POST: api/shoppingcart/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto checkoutDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any()) return BadRequest("Cart is empty.");

            // Calculate total amount for the order
            decimal totalAmount = 0;
            foreach (var cartItem in cart.CartItems)
            {
                totalAmount += cartItem.Book.Price * cartItem.Quantity;
            }

            // Create a new order
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                OrderDate = DateTime.UtcNow,
                RecipientName = checkoutDto.RecipientName,
                ShippingAddress = checkoutDto.ShippingAddress,
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Quantity = ci.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems); // Clear the cart after checkout
            await _context.SaveChangesAsync();

            return Ok(new { OrderId = order.Id, Message = "Checkout successful.", TotalAmount = totalAmount });
        }
    }
}
