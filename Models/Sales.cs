using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SalesUON.Models
{
    public class Sale
    {
        public string Make { get; set; }
        public string Model { get; set; }
        [Key]
        public string VIN { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
