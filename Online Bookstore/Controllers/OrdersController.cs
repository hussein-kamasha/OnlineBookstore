using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Bookstore.Data;
using Online_Bookstore.Dtos;
using Online_Bookstore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Online_Bookstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/orders
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book) // Include book details for each order item
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return Ok(new { Message = "No orders found." });
            }

            var orderDtos = orders.Select(o => new OrderDto
            {
                OrderId = o.Id,
                TotalAmount = o.TotalAmount,
                ShippingAddress = o.ShippingAddress,
                RecipientName = o.RecipientName,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    BookId = oi.BookId,
                    BookTitle = oi.Book.Title,
                    Quantity = oi.Quantity
                }).ToList(),
                OrderDate = o.OrderDate 
            }).ToList();

            return Ok(orderDtos);
        }

    }
}
