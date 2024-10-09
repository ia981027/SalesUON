using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SalesUON.Data;
using SalesUON.Models;


namespace SalesUON.Controllers
{
    [ApiController]
    [Route("api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public SalesController(MyDbContext
 context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Sale> GetSales()
        {
            return _context.Sale.ToList();
        }
    }
}