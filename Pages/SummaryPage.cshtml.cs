using Microsoft.AspNetCore.Mvc.RazorPages;
using SalesUON.Models;
using System.Net.Http.Json;
using System.Collections.Generic; // Add this for List<T>

namespace SalesUON.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly HttpClient _http;

        public SummaryModel(IHttpClientFactory clientFactory)
        {
            _http = clientFactory.CreateClient("SalesAPI");
        }

        public List<SalesSummaryByMonth> MonthSummaryData { get; set; }
        public List<SalesSummaryByYear> YearSummaryData { get; set; }
        public List<SalesSummaryByMake> MakeSummaryData { get; set; }
        public List<SalesSummaryByModel> ModelSummaryData { get; set; }

        public async Task OnGetAsync()
        {
            MonthSummaryData = await _http.GetFromJsonAsync<List<SalesSummaryByMonth>>("/api/csv/sales/summary/month");
            YearSummaryData = await _http.GetFromJsonAsync<List<SalesSummaryByYear>>("/api/csv/sales/summary/year");
            MakeSummaryData = await _http.GetFromJsonAsync<List<SalesSummaryByMake>>("/api/csv/sales/summary/make");
            ModelSummaryData = await _http.GetFromJsonAsync<List<SalesSummaryByModel>>("/api/csv/sales/summary/model");
        }
    }

    // Data structures for the summaries (you might need to adjust these)
    public class SalesSummaryByMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalSales { get; set; }
    }

    public class SalesSummaryByYear
    {
        public int Year { get; set; }
        public int TotalSales { get; set; }
    }

    public class SalesSummaryByMake
    {
        public string Make { get; set; }
        public int TotalSales { get; set; }
    }

    public class SalesSummaryByModel
    {
        public string Model { get; set; }
        public int TotalSales { get; set; }
    }
}
