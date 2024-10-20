namespace SalesUON.Models // Use the correct namespace for your project
{
    public class SalesSummary 
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int TotalSales { get; set; }
    }
}
