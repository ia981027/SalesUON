using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SalesUON.Models
{
    public class Sale
    {
        [Key]
        public string Make { get; set; }
        public string Model { get; set; }
        public string VIN { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
