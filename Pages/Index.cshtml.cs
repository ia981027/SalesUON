using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SalesUON.Data;
using SalesUON.Models;

namespace SalesUON.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly HttpClient _http;

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _http = clientFactory.CreateClient("SalesAPI");
        }

        public List<Sale>? Sales { get; set; }
        public int CurrentPage { get; set; } = 1;
        private const int PageSize = 50;

        public async Task OnGetAsync(int page = 1)
        {
            CurrentPage = page;
            Sales = await _http.GetFromJsonAsync<List<Sale>>($"/api/csv/sales?page={page}&pageSize={PageSize}");
        }
    }
}