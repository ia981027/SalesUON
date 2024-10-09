using Microsoft.EntityFrameworkCore;
using SalesUON.Models;

namespace SalesUON.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Sale> Sale { get; set; }

    }
}