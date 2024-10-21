using Microsoft.AspNetCore.Mvc;
using SalesUON.Data;
using SalesUON.Models;
using Microsoft.EntityFrameworkCore; // Import for pagination

namespace SalesUON.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public SalesController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet("sales")]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales(int page = 1, int pageSize = 50, DateTime? saleDate = null, string make = null, string model = null)
        {
            try
            {
                var query = _context.Sale.AsQueryable(); // Start with the DbSet

                if (saleDate.HasValue)
                {
                    query = query.Where(s => s.SaleDate == saleDate.Value);
                }
                if (!string.IsNullOrEmpty(make))
                {
                    query = query.Where(s => s.Make == make);
                }
                if (!string.IsNullOrEmpty(model))
                {
                    query = query.Where(s => s.Model == model);
                }

                var sales = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(sales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}