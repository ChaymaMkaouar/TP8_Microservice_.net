using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersAPIContext _context;

        public OrdersController(OrdersAPIContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orders>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/products  -> produits synchronisés via RabbitMQ
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetSyncedProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Orders>> GetOrders(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders == null) return NotFound();
            return orders;
        }

        // PUT: api/Orders/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutOrders(int id, Orders orders)
        {
            if (id != orders.OrdersId) return BadRequest();

            _context.Entry(orders).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.OrdersId == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<Orders>> PostOrders(Orders orders)
        {
            // Le produit doit avoir été synchronisé depuis ProductsAPI
            if (!await _context.Products.AnyAsync(p => p.Id == orders.ProductId))
                return BadRequest($"Produit {orders.ProductId} inconnu dans OrdersAPI (non synchronisé).");

            if (orders.OrderDate == default) orders.OrderDate = DateTime.UtcNow;

            _context.Orders.Add(orders);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetOrders", new { id = orders.OrdersId }, orders);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrders(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders == null) return NotFound();

            _context.Orders.Remove(orders);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
